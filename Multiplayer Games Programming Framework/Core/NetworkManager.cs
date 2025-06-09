#nullable enable

#region Includes

// Includes
using Microsoft.Xna.Framework;
using Multiplayer_Games_Programming_Packet_Library;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Color = System.Drawing.Color;

namespace Multiplayer_Games_Programming_Framework.Core;

#endregion Includes

internal class NetworkManager
{
    #region Network Managment Variables

    private static NetworkManager? Instance;
    public readonly TcpClient m_tcpClient;
    public readonly UdpClient m_udpClient;
    private readonly RSACryptoServiceProvider m_rsaProvider;
    public RSAParameters m_privateKey;
    public RSAParameters m_publicKey;
    public RSAParameters m_serverPublicKey;
    public StreamReader m_reader = null!;
    public StreamWriter m_writer = null!;
    public bool m_resetLobby;
    public bool m_quit;

    #endregion Network Managment Variables

    #region Client Managment Varibles

    // All the variables I use for the client management
    public Color m_playerColour = Color.White;

    public Dictionary<int, Action<Vector2>> m_playerPositions = null!;
    public int m_clientID = -1;
    public bool m_connected = false;
    public bool m_emoteThreadStop = false;
    public bool m_inLobby1 = false;
    public bool m_inLobby2 = false;
    public bool m_inLobby3 = false;
    public int m_lobby1Size = 0;
    public int m_lobby2Size = 0;
    public int m_lobby3Size = 0;
    public int m_lobbyScore = 0;
    public bool m_loggedin = false;
    public bool m_startLobby1 = false;
    public bool m_startLobby2 = false;
    public bool m_startLobby3 = false;
    public string m_username = "NULL";

    #endregion Client Managment Varibles

    #region Opponent Management Variables

    // All the variables I use for the opponent management
    public Color m_oppColour = Color.White;

    public bool[] m_oppEmote = new bool[6];
    public int m_oppID = -1;
    public MovementDirection m_oppMovementDirection = MovementDirection.None;
    public string m_oppUsername = null!;
    public bool m_oppColourInfoConfirmed = false;
    public bool m_oppUsernameInfoConfirmed = false;

    #endregion Opponent Management Variables

    #region Constructor

    private NetworkManager()
    {
        m_tcpClient = new TcpClient();
        m_udpClient = new UdpClient();
        m_rsaProvider = new RSACryptoServiceProvider(2048); // 2048 bit Encryption
    }

    // Get instance of the network manager
    public static NetworkManager? m_Instance
    {
        get
        {
            if (Instance == null) return Instance = new NetworkManager();

            return Instance;
        }
    }

    #endregion Constructor

    #region Initialisation Methods

    // Connect to the server
    public bool Connect(string ip, int port)
    {
        try
        {
            // Activate the TCP client
            m_tcpClient.Connect(IPAddress.Parse(ip), port);

            // Activate the UDP client
            m_udpClient.Connect(IPAddress.Parse(ip), port);

            // Activate the reader and writer
            m_writer = new StreamWriter(m_tcpClient.GetStream(), Encoding.UTF8);

            m_reader = new StreamReader(m_tcpClient.GetStream(), Encoding.UTF8);

            // Start the TCP and UDP threads
            Run();

            return true;
        }// Catch socket exceptions
        catch (SocketException ex)
        {
            Debug.WriteLine(ex.Message);
        }

        return false;
    }

    public void Run()
    {
        // Create a dictionary to store the player positions
        m_playerPositions = new Dictionary<int, Action<Vector2>>();

        // Start the TCP server response processing thread
        var tcpServerResponseProcessing = new Thread(TcpProcessServerResponse)
        {
            Name = "TCP Process Server Response"
        };

        tcpServerResponseProcessing.Start();

        // Start the UDP listen thread
        _ = UdpListen();
    }

    // Close the network
    public void Close()
    {
        try
        {
            // Close the emote thread
            m_emoteThreadStop = true;

            if (m_tcpClient != null)
            {
                m_tcpClient.Close();
            }

            if (m_udpClient != null)
            {
                m_udpClient.Close();
            }

            if (m_reader != null)
            {
                m_reader.Close();
            }

            if (m_writer != null)
            {
                m_writer.Close();
            }

            // Clear the player positions
            if (m_playerPositions != null)
            {
                m_playerPositions.Clear();
            }
        }
        catch (Exception ex)
        { Debug.WriteLine(ex.Message); }
    }

    public void Login()
    {
        // Lock the network manager login method
        lock (this)
        {
            // Get the IP address of the client
            var ip = ((IPEndPoint)m_tcpClient.Client.RemoteEndPoint!)?.Address.ToString();

            // Send a message packet to the server
            var messagePacket = new MessagePacket("CLIENT: A Client from:" + ip + " is trying to connect")
            {
                m_Name = "Login"
            };

            // Send the message packet
            TcpSendPacket(messagePacket, false);
        }
    }

    #endregion Initialisation Methods

    #region UDP Methods

    // Listen for any UDP packets
    private async Task UdpListen()
    {
        // Since the player has both a TCP and UDP connection, I can just use the TCP connection to check if the player is connected
        // ...Look it beats using a while true loop
        while (m_tcpClient.Connected)
            try
            {
                var receiveResult = await m_udpClient.ReceiveAsync();
                var receivedData = receiveResult.Buffer;
                var message = Packet.Deserialize(Encoding.UTF8.GetString(receivedData, 0, receivedData.Length));
                // Handle the UDP packet
                if (message != null) HandleUdpPacket(message);
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.Message);
            }
    }

    private void HandleUdpPacket(Packet? message)
    {
        if (message == null) return;

        // Switch on the packet type
        switch (message.m_packetType)
        {
            // If the packet is encrypted then decrypt it and recursively call the method
            case Packet.PacketType.Encrypted:
                var encryptedPacket = (EncryptedPacket)message;

                if (encryptedPacket.m_Encryption != null)
                {
                    var decryptedPacket = Decrypt(encryptedPacket.m_Encryption);

                    message = decryptedPacket;
                    HandleUdpPacket(message);
                }

                break;

            // If the packet is a position packet then handle it
            case Packet.PacketType.Position:
                UdpHandlePositionPacket((Position)message);
                break;

            case Packet.PacketType.Empty:
            case Packet.PacketType.MessagePacket:
            case Packet.PacketType.ClientId:
            case Packet.PacketType.LobbySize:
            case Packet.PacketType.InLobby:
            case Packet.PacketType.StartLobby:
            case Packet.PacketType.Colour:
            case Packet.PacketType.Disconnect:
            case Packet.PacketType.Heartbeat:
            case Packet.PacketType.LobbyScore:
            case Packet.PacketType.Username:
            case Packet.PacketType.Emote:
            case Packet.PacketType.ResetGameObjects:
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    // Handle the position packet
    private void UdpHandlePositionPacket(Position position)
    {
        // If the player is not in the dictionary then return
        if (!m_playerPositions.ContainsKey(position.m_ID)) return;

        // Invoke the player position
        m_playerPositions[position.m_ID]?.Invoke(new Vector2(position.m_XPOS, position.m_YPOS));

        // Set the opponent movement direction for prediction purposes
        m_oppMovementDirection = position.m_direction;
    }

    // Send a UDP packet
    public void UdpSendPacket(Packet message, bool sendEncrypted)
    {
        try
        {
            lock (this)
            {
                // Convert the packet to a JSON string
                var sentMessage = message.ToJson();

                // If the packet is encrypted then encrypt it
                if (sendEncrypted) sentMessage = new EncryptedPacket(m_clientID, Encrypted(message)).ToJson();

                // Convert the JSON string to bytes and send it
                var bytes = Encoding.UTF8.GetBytes(sentMessage);
                m_udpClient.Send(bytes, bytes.Length);
                Debug.WriteLine("UDP Message Sent: " + sentMessage);
            }
        }
        catch (SocketException ex)
        {
            Debug.WriteLine("Client UDP Send Method exception: " + ex.Message);
        }
    }

    #endregion UDP Methods

    #region TCP Methods

    // Handle the TCP packet
    private void TcpHandlePacket(Packet? message)
    {
        lock (this)
        {
            if (message == null) return;

            // Switch on the packet type
            switch (message.m_packetType)
            {
                // If the packet is a reset game objects packet then handle it
                case Packet.PacketType.ResetGameObjects:
                    TcpHandleResetGameObjects((ResetGameObjects)message);
                    break;

                // If the packet is a username packet then handle it
                case Packet.PacketType.Username:
                    TcpHandleUsernamePacket((Username)message);
                    break;

                // If the packet is a lobby score packet then handle it
                case Packet.PacketType.LobbyScore:
                    TcpHandleLobbyScore((LobbyScore)message);
                    break;

                // If the packet is an encrypted packet then decrypt it and recursively call the method
                case Packet.PacketType.Encrypted:
                    var encryptedPacket = (EncryptedPacket)message;
                    if (encryptedPacket.m_Encryption != null)
                    {
                        var decryptedPacket = Decrypt(encryptedPacket.m_Encryption);

                        TcpHandlePacket(decryptedPacket);
                    }
                    break;

                // If the packet is an emote packet then handle it
                case Packet.PacketType.Emote:
                    TcpHandleEmotePacket((Emote)message);
                    break;

                // If the packet is a message packet then handle it
                case Packet.PacketType.MessagePacket:
                    TcpHandleMessagePacket((MessagePacket)message);
                    break;

                // If the packet is a position packet then do nothing
                case Packet.PacketType.Empty:
                    // Empty packet
                    break;

                // If the packet is a client ID packet then handle it
                case Packet.PacketType.ClientId:
                    TcpHandleClientIdPacket((ClientID)message);
                    break;

                // If the packet is a lobby size packet then handle it
                case Packet.PacketType.LobbySize:
                    TcpHandleLobbySizePacket((LobbySize)message);
                    break;

                // If the packet is a start lobby packet then handle it
                case Packet.PacketType.StartLobby:
                    TcpHandleStartLobbyPacket((startLobby)message);
                    break;
                // If the packet is a colour packet then handle it
                case Packet.PacketType.Colour:
                    TcpHandleOppInfoPacket((Colour)message);
                    break;

                // If the packet is a disconnect packet then start the quit process
                case Packet.PacketType.Disconnect:
                    m_quit = true;
                    break;

                case Packet.PacketType.Position:
                case Packet.PacketType.InLobby:
                case Packet.PacketType.Heartbeat:
                default:
                    Debug.WriteLine("Unknown packet type received.");
                    break;
            }
        }
    }

    // Process a tcp server response
    private void TcpProcessServerResponse()
    {
        try
        {
            while (m_tcpClient.Connected)
                try
                {
                    // Read the packet
                    Packet? message = null;
                    if ((message = Read()) == null) continue;

                    // Handle the packet
                    TcpHandlePacket(message);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Exception: {ex.Message}");
                }
        }
        catch (SocketException ex)
        {
            Debug.WriteLine($"Socket Exception: {ex.Message}");
        }
        finally
        {
            // Close the network if the thread ends
            Close();
        }
    }

    // Flip a bool for the Gamescene to reset the game objects
    private void TcpHandleResetGameObjects(ResetGameObjects reset)
    {
        m_resetLobby = true;
    }

    // Opponent emote packet
    private void TcpHandleEmotePacket(Emote emote)
    {
        // Set all other opponent emotes to false
        for (var i = 0; i < m_oppEmote.Length; i++) m_oppEmote[i] = false;

        // Set the emote to true
        m_oppEmote[emote.m_emote] = true;
    }

    // Opponent username packet
    private void TcpHandleUsernamePacket(Username username)
    {
        // Set the opponent username
        m_oppUsername = username.m_Username;

        // Set the opponent username info confirmed to true so that the event listener can be removed
        m_oppUsernameInfoConfirmed = true;
    }

    // Add to the score and reset the lobby
    private void TcpHandleLobbyScore(LobbyScore messagePacket)
    {
        m_lobbyScore = messagePacket.m_Score;
        m_resetLobby = true;
    }

    // Handle a message packet
    private static void TcpHandleMessagePacket(MessagePacket messagePacket)
    {
        Debug.WriteLine(messagePacket.m_Message);
    }

    // Handle the opponent colour packet
    private void TcpHandleOppInfoPacket(Colour colour)
    {
        // Set the opponent colour
        m_oppColour = Color.FromArgb((int)colour.m_Colour_A, (int)colour.m_Colour_R, (int)colour.m_Colour_G,
            (int)colour.m_Colour_B);

        // Set the opponent colour info confirmed to true so that the event listener can be removed
        m_oppColourInfoConfirmed = true;
    }

    // Handle the client ID packet
    private void TcpHandleClientIdPacket(ClientID clientId)
    {
        switch (clientId.m_lobby)
        {
            // If the client is in lobby 0 then set the client ID
            case 0:
                Debug.WriteLine(clientId.m_ID);
                m_clientID = clientId.m_ID;
                m_publicKey = clientId.m_publicKey;
                m_privateKey = clientId.m_privateKey;
                m_serverPublicKey = clientId.m_serverPublicKey;
                var confirmationPacket = new MessagePacket("CLIENT: Confirmed Client ID:" + m_clientID, m_clientID);
                TcpSendPacket(confirmationPacket, true);
                UdpSendPacket(confirmationPacket, true);

                m_connected = true;
                break;

            // If the client is in lobby 1/2/3 then set the opponent ID
            case 1:
                if (m_inLobby1) m_oppID = clientId.m_ID;
                break;

            case 2:
                if (m_inLobby2) m_oppID = clientId.m_ID;
                break;

            case 3:
                if (m_inLobby3) m_oppID = clientId.m_ID;
                break;
        }
    }

    // Handle the in lobby packet
    private void TcpHandleLobbySizePacket(LobbySize lobbySize)
    {
        // Add to the lobby size based on the lobby
        switch (lobbySize.m_lobby)
        {
            case 1:
                m_lobby1Size = lobbySize.m_Size;
                break;

            case 2:
                m_lobby2Size = lobbySize.m_Size;
                break;

            case 3:
                m_lobby3Size = lobbySize.m_Size;
                break;
        }
    }

    // Handle the start lobby packet
    private void TcpHandleStartLobbyPacket(startLobby startLobby)
    {
        // Set the start lobby bool based on the lobby
        if (m_inLobby1) m_startLobby1 = startLobby.m_Ready;
        if (m_inLobby2) m_startLobby2 = startLobby.m_Ready;
        if (m_inLobby3) m_startLobby3 = startLobby.m_Ready;
    }

    // Send a TCP packet
    public void TcpSendPacket(Packet message, bool sendEncrypted)
    {
        try
        {
            // Lock the network manager send method
            lock (this)
            {
                // Convert the packet to a JSON string
                var sentMessage = message.ToJson();

                // If the packet is encrypted then encrypt it
                if (sendEncrypted) sentMessage = new EncryptedPacket(m_clientID, Encrypted(message)).ToJson();

                try
                {
                    // Write the JSON string to the stream
                    m_writer.WriteLine(sentMessage);
                    Debug.WriteLine(sentMessage);

                    // Flush the stream
                    m_writer.Flush();
                }
                catch (IOException ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }
        catch (SocketException ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }

    // Read a packet
    public Packet? Read()
    {
        // Read the JSON string from the stream
        var json = m_reader.ReadLine();

        // Deserialise the JSON string to a packet
        return Packet.Deserialize(json);
    }

    #endregion TCP Methods

    #region Encryption and Decryption Methods

    // Encrypt a packet
    public byte[] Encrypted(Packet message)
    {
        // Lock the RSA provider, so only one thread can access it at a time
        lock (m_rsaProvider)
        {
            // Import the server's public key
            m_rsaProvider.ImportParameters(m_serverPublicKey);

            // Convert the packet to a JSON string
            var json = message.ToJson();

            // Encrypt the JSON string
            var encrypted = m_rsaProvider.Encrypt(Encoding.UTF8.GetBytes(json), false);
            return encrypted;
        }
    }

    // Decrypt a packet
    public Packet? Decrypt(byte[] encrypted)
    {
        // Lock the RSA provider, so only one thread can access it at a time
        lock (m_rsaProvider)
        {
            // Import the client's private key
            m_rsaProvider.ImportParameters(m_privateKey);

            // Decrypt the packet
            var decrypted = m_rsaProvider.Decrypt(encrypted, false);

            // Convert the decrypted packet to a JSON string
            var json = Encoding.UTF8.GetString(decrypted);

            // Deserialise the JSON string to a packet
            var packet = Packet.Deserialize(json);
            return packet;
        }
    }

    #endregion Encryption and Decryption Methods
}