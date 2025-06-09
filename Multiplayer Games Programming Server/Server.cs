#region Includes

// Includes
using Multiplayer_Games_Programming_Packet_Library;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace Multiplayer_Games_Programming_Server;

#endregion

internal class Server
{
    #region Member Variables

    //Connection Variables
    private readonly TcpListener m_TcpListener;

    private readonly UdpClient m_UdpListener;
    private readonly ConcurrentDictionary<int, ConnectedClient> m_Clients;
    private readonly Semaphore m_connectionSemaphore;
    private readonly ConcurrentBag<Packet> m_Packets;

    // Encryption Variables
    private readonly RSACryptoServiceProvider m_RsaProvider;

    private readonly RSAParameters m_ServerPrivateKey;
    private readonly RSAParameters m_serverPublicKey;

    // Lobby Variables
    private int m_lobby1ready;

    private int m_lobby1Score;
    private int m_lobby1Size;
    private int m_lobby2ready;
    private int m_lobby2Score;
    private int m_lobby2Size;
    private int m_lobby3ready;
    private int m_lobby3Score;
    private int m_lobby3Size;

    #endregion

    #region Constructor

    public Server(string ipAddress, int port)
    {
        m_TcpListener = new TcpListener(IPAddress.Parse(ipAddress), port); // Listen on a port and IP for TCP packets
        m_UdpListener = new UdpClient(port); // Listen on a port for UDP packets

        m_Clients = new ConcurrentDictionary<int, ConnectedClient>(); // Store all connected clients

        m_Packets = new ConcurrentBag<Packet>(); // Store all packets

        m_connectionSemaphore =
            new Semaphore(8, 8); // 8 clients max // 6 concurrent clients playing, two waiting in lobby to play

        m_RsaProvider = new RSACryptoServiceProvider(2048); // 2048 bit encryption

        m_serverPublicKey = m_RsaProvider.ExportParameters(false); // Export the public key

        m_ServerPrivateKey = m_RsaProvider.ExportParameters(true); // Export the private key
    }

    public void Start()
    {
        var id = 0;

        try
        {
            // Start the server listening
            m_TcpListener.Start();
            Console.WriteLine("SERVER: Server is Listening...");

            while (true)
            {
                // Putting a semaphore here means that the server will only accept 8 clients at a time but when a client disconnects,
                // the next client in the queue will be accepted.
                m_connectionSemaphore.WaitOne();

                // Accept the client
                var socket = m_TcpListener.AcceptSocket();
                Console.WriteLine($"SERVER: Connection made from {socket.RemoteEndPoint}");

                var clientId = id++;

                // Add the client to the dictionary
                m_Clients.TryAdd(clientId, new ConnectedClient(socket));

                // Start the client threads
                var clientThread = new Thread(() =>
                {
                    try
                    {
                        TcpListen(clientId);
                    }
                    finally
                    {
                        m_connectionSemaphore.Release();
                    }
                });

                // Start the heartbeat thread
                var heartbeatThread = new Thread(() => { HeartBeatThread(clientId); });

                heartbeatThread.Start();
                clientThread.Start();

                // Start the UDP listener
                _ = UdpListen(clientId);
            }
        }
        catch (SocketException ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            Stop();
            m_connectionSemaphore.Close();
        }
    }

    // Stop the server listening
    public void Stop()
    {
        m_TcpListener.Stop();
        m_UdpListener.Close();
    }

    #endregion

    #region Heartbeat Thread

    // This heartbeat thread will check if the client has sent a heartbeat within 12 seconds
    // If not, the client will be disconnected
    private void HeartBeatThread(int id)
    {
        while (true)
        {
            // If the client is not in the dictionary, break
            if (!m_Clients.TryGetValue(id, out _)) continue;

            // Increase the heartbeat timer
            m_Clients[id].m_heartbeatTimer++;
            if (m_Clients[id].m_heartbeatTimer >= 12) break;

            // Wait for 1 second
            Thread.Sleep(1000);
        }

        try
        {
            // Try to send a disconnect packet to the client if they reconnect
            m_Clients[id].TcpSendPacket(new Disconnect(), true);
        }
        catch (SocketException ex)
        {
            Console.WriteLine(ex.Message);
        }

        Console.WriteLine("SERVER: Client ID: " + id +
                          " has disconnected - no heartbeat received within 12 seconds.");

        // Close the client
        TcpHandleDisconnect(id);
    }

    private void HandleHeartbeat(int id, Heartbeat message)
    {
        if (m_Clients.TryGetValue(id, out _) == false) return;
        Console.WriteLine("SERVER: Client ID: " + message.m_ID + " Heartbeat received.");

        // Reset the heartbeat timer
        m_Clients[message.m_ID].m_heartbeatTimer = 0;
    }

    #endregion

    #region UDP Methods

    private async Task UdpListen(int id)
    {
        while (m_Clients.TryGetValue(id, out _) && m_Clients[id].m_socket.Connected)
            try
            {
                // Receive the UDP packet
                var receiveResult = await m_UdpListener.ReceiveAsync();

                // Deserialize the packet
                var receivedData = receiveResult.Buffer;
                var message = Packet.Deserialize(Encoding.UTF8.GetString(receivedData, 0, receivedData.Length));

                // Add the packet to the packet queue
                if (message == null) continue;
                m_Packets.Add(message);

                // Handle the packet
                HandleUdpPacket(id, message, receiveResult);

                // Remove the packet from the queue
                m_Packets.TryTake(out message);
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.Message);
            }
    }

    private void HandleUdpPacket(int id, Packet? message, UdpReceiveResult receiveResult)
    {
        if (message == null) return;

        // Switch on the packet type
        switch (message.m_packetType)
        {
            case Packet.PacketType.Encrypted:
                var encryptedPacket = (EncryptedPacket)message;

                // If the encryption is not null
                if (encryptedPacket.m_Encryption != null)
                {
                    // Decrypt the packet
                    var decryptedPacket = m_Clients[id].Decrypt(encryptedPacket.m_Encryption);

                    // Recursively call the method
                    HandleUdpPacket(id, decryptedPacket, receiveResult);
                }

                break;

            case Packet.PacketType.Position:
                UdpHandlePosition(id, (Position)message);
                break;

            case Packet.PacketType.MessagePacket:
                UdpConfirmClientId((MessagePacket)message, receiveResult);

                break;

            case Packet.PacketType.Heartbeat:
                HandleHeartbeat(id, (Heartbeat)message);

                break;

            case Packet.PacketType.Empty:
            case Packet.PacketType.ClientId:
            case Packet.PacketType.LobbySize:
            case Packet.PacketType.InLobby:
            case Packet.PacketType.StartLobby:
            case Packet.PacketType.Colour:
            case Packet.PacketType.Disconnect:
            case Packet.PacketType.LobbyScore:
            case Packet.PacketType.Username:
            case Packet.PacketType.Emote:
            case Packet.PacketType.ResetGameObjects:
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void UdpConfirmClientId(MessagePacket message, UdpReceiveResult receiveResult)
    {
        if (m_Clients.TryGetValue(message.m_Id, out _) == false) return;
        if (message.m_Id == -1) return;
        // Confirm the client's endpoint and save it
        m_Clients[message.m_Id].m_result = receiveResult;
        Console.WriteLine("SERVER: UDP Client ID: " + message.m_Id +
                          " :Confirmed Endpoint = " +
                          m_Clients[message.m_Id].m_result.RemoteEndPoint);
    }

    private void UdpHandlePosition(int id, Position message)
    {
        if (m_Clients.TryGetValue(id, out _) == false) return;

        // Switch on the lobby
        switch (message.m_Lobby)
        {
            case 1:
                // Send the packet to all clients in the lobby
                foreach (var client in m_Clients.Values)
                    if (client.m_inLobby1 && client.m_id != id && m_Clients.TryGetValue(id, out _))

                        client.UdpSendPacket(message, m_UdpListener, true);
                break;

            case 2:
                foreach (var client in m_Clients.Values)
                    if (client.m_inLobby2 && client.m_id != id && m_Clients.TryGetValue(id, out _))
                        client.UdpSendPacket(message, m_UdpListener, true);
                break;

            case 3:
                foreach (var client in m_Clients.Values)
                    if (client.m_inLobby3 && client.m_id != id && m_Clients.TryGetValue(id, out _))
                        client.UdpSendPacket(message, m_UdpListener, true);
                break;
        }
    }

    #endregion

    #region TCP Methods

    private void TcpListen(int id)
    {
        // Set the client's ID
        m_Clients[id].m_id = id;

        // Generate the client's keys
        m_Clients[id].GenerateKeys(m_serverPublicKey, m_ServerPrivateKey);

        while (m_Clients.TryGetValue(id, out _) && m_Clients[id].m_socket.Connected)
            try
            {
                Packet? packet;
                if ((packet = m_Clients[id].Read()) == null) continue;

                // Add the packet to the packet queue
                m_Packets.Add(packet);

                // Handle the packet
                HandleTcpPacket(id, packet);

                // Remove the packet from the queue
                m_Packets.TryTake(out packet);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
    }

    private void HandleTcpPacket(int id, Packet? message)
    {
        if (message == null) return;
        switch (message.m_packetType)
        {
            case Packet.PacketType.ResetGameObjects:

                TcpHandleResetGameObjects(id, (ResetGameObjects)message);

                break;

            case Packet.PacketType.Username:

                TcpHandleUsername(id, (Username)message);

                break;

            case Packet.PacketType.Emote:
                TcpHandleEmote(id, (Emote)message);
                break;

            case Packet.PacketType.LobbyScore:

                TcpHandleLobbyScore(id, (LobbyScore)message);

                break;

            case Packet.PacketType.Encrypted:
                var encryptedPacket = (EncryptedPacket)message;

                // If the encryption is not null
                if (encryptedPacket.m_Encryption != null)
                {
                    // Decrypt the packet
                    var decryptedPacket = m_Clients[id].Decrypt(encryptedPacket.m_Encryption);

                    // Recursively call the method
                    HandleTcpPacket(id, decryptedPacket);
                }

                break;

            case Packet.PacketType.Colour:
                TcpHandleColour(id, (Colour)message);
                break;

            case Packet.PacketType.Disconnect:
                TcpHandleDisconnect(id);
                break;

            case Packet.PacketType.MessagePacket:
                TcpHandleMessagePacket((MessagePacket)message);
                break;

            case Packet.PacketType.Empty:
                HandleEmptyPacket((EmptyPacket)message);
                break;

            case Packet.PacketType.LobbySize:
                TcpHandleLobbySize(id, (LobbySize)message);
                break;

            case Packet.PacketType.InLobby:
                TcpHandleInLobby((inLobby)message);
                break;

            case Packet.PacketType.StartLobby:
                TcpHandleStartLobby((startLobby)message);
                break;

            case Packet.PacketType.ClientId:
                TcpHandleClientId(id, (ClientID)message);
                break;

            case Packet.PacketType.Heartbeat:
                HandleHeartbeat(id, (Heartbeat)message);

                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void TcpHandleResetGameObjects(int id, ResetGameObjects message)
    {
        // Send the packet to all clients in the lobby
        foreach (var client in m_Clients.Values)
            switch (message.m_lobbyID)
            {
                case 1:
                    if (client.m_inLobby1 && m_Clients.TryGetValue(id, out _))
                        client.TcpSendPacket(message, true);

                    break;

                case 2:
                    if (client.m_inLobby2 && m_Clients.TryGetValue(id, out _))
                        client.TcpSendPacket(message, true);
                    break;

                case 3:

                    if (client.m_inLobby3 && m_Clients.TryGetValue(id, out _))
                        client.TcpSendPacket(message, true);
                    break;
            }
    }

    private void TcpHandleEmote(int id, Emote message)
    {
        // Send the packet to all clients in the lobby
        foreach (var client in m_Clients.Values)
            switch (message.m_lobbyID)
            {
                case 1:
                    if (client.m_inLobby1 && client.m_id != id && m_Clients.TryGetValue(id, out _))
                        client.TcpSendPacket(message, true);

                    break;

                case 2:
                    if (client.m_inLobby2 && client.m_id != id && m_Clients.TryGetValue(id, out _))
                        client.TcpSendPacket(message, true);
                    break;

                case 3:

                    if (client.m_inLobby3 && client.m_id != id && m_Clients.TryGetValue(id, out _))
                        client.TcpSendPacket(message, true);
                    break;
            }
    }

    // Send the packet to all clients in the lobby
    private void TcpHandleUsername(int id, Username message)
    {
        foreach (var client in m_Clients.Values)
            switch (message.m_lobbyID)
            {
                case 1:
                    if (client.m_inLobby1 && client.m_id != id && m_Clients.TryGetValue(id, out _))
                        client.TcpSendPacket(message, true);

                    break;

                case 2:
                    if (client.m_inLobby2 && client.m_id != id && m_Clients.TryGetValue(id, out _))
                        client.TcpSendPacket(message, true);
                    break;

                case 3:

                    if (client.m_inLobby3 && client.m_id != id && m_Clients.TryGetValue(id, out _))
                        client.TcpSendPacket(message, true);
                    break;
            }
    }

    private void TcpHandleLobbyScore(int id, LobbyScore message)
    {
        if (m_Clients.TryGetValue(id, out _) == false) return;

        switch (message.m_lobbyID)
        {
            case 1:
                // If the score is the same, return
                if (Interlocked.CompareExchange(ref m_lobby1Score, 0, 0) == message.m_Score) return;

                // Set the score atomically to prevent race conditions
                Interlocked.Exchange(ref m_lobby1Score, message.m_Score);

                foreach (var client in m_Clients.Values)
                    if (client.m_inLobby1 && client.m_id != id && m_Clients.TryGetValue(id, out _))
                    {
                        // Send the packet to all clients in the lobby
                        var lobbyScore = new LobbyScore(Interlocked.CompareExchange(ref m_lobby1Score, 0, 0));
                        client.TcpSendPacket(lobbyScore, true);
                    }

                break;

            case 2:
                if (Interlocked.CompareExchange(ref m_lobby2Score, 0, 0) == message.m_Score) return;
                Interlocked.Exchange(ref m_lobby2Score, message.m_Score);
                foreach (var client in m_Clients.Values)
                    if (client.m_inLobby2 && client.m_id != id && m_Clients.TryGetValue(id, out _))
                    {
                        var lobbyScore = new LobbyScore(Interlocked.CompareExchange(ref m_lobby2Score, 0, 0));
                        client.TcpSendPacket(lobbyScore, true);
                    }

                break;

            case 3:
                if (Interlocked.CompareExchange(ref m_lobby3Score, 0, 0) == message.m_Score) return;
                Interlocked.Exchange(ref m_lobby3Score, message.m_Score);
                foreach (var client in m_Clients.Values)
                    if (client.m_inLobby3 && client.m_id != id && m_Clients.TryGetValue(id, out _))
                    {
                        var lobbyScore = new LobbyScore(Interlocked.CompareExchange(ref m_lobby3Score, 0, 0));
                        client.TcpSendPacket(lobbyScore, true);
                    }

                break;
        }
    }

    private void TcpHandleDisconnect(int id)
    {
        if (m_Clients.TryGetValue(id, out _) == false) return;

        if (m_Clients[id].m_inLobby1)
        {
            // Remove the client from the lobby
            Interlocked.Add(ref m_lobby1Size, -1);

            // Reset the lobby ready status
            Interlocked.Exchange(ref m_lobby1ready, 0);
            foreach (var client in m_Clients.Values)
            {
                if (client.m_id == id) continue;

                // Send the lobby size packet to all clients in the lobby
                var lobby1SizePacket = new LobbySize(Interlocked.CompareExchange(ref m_lobby1Size, 0, 0), 1);
                client.TcpSendPacket(lobby1SizePacket, true);
            }
        }

        if (m_Clients[id].m_inLobby2)
        {
            Interlocked.Add(ref m_lobby2Size, -1);
            Interlocked.Exchange(ref m_lobby2ready, 0);
            foreach (var client in m_Clients.Values)
            {
                if (client.m_id == id) continue;
                var lobby2SizePacket = new LobbySize(Interlocked.CompareExchange(ref m_lobby2Size, 0, 0), 1);
                client.TcpSendPacket(lobby2SizePacket, true);
            }
        }

        if (m_Clients[id].m_inLobby3)
        {
            Interlocked.Add(ref m_lobby3Size, -1);
            Interlocked.Exchange(ref m_lobby3ready, 0);
            foreach (var client in m_Clients.Values)
            {
                if (client.m_id == id) continue;
                var lobby3SizePacket = new LobbySize(Interlocked.CompareExchange(ref m_lobby3Size, 0, 0), 1);
                client.TcpSendPacket(lobby3SizePacket, true);
            }
        }

        Console.WriteLine("SERVER: Client ID: " + id + " left.");

        // Close the client
        m_Clients[id].Close();

        // Remove the client from the dictionary
        m_Clients.TryRemove(id, out _);
    }

    private static void TcpHandleMessagePacket(MessagePacket message)
    {
        // Print the message
        Console.WriteLine(message.m_Message);
    }

    private void HandleEmptyPacket(EmptyPacket message)
    {
        // Print the message
        Console.WriteLine("EMPTYPACKETGET");
    }

    private void TcpHandleColour(int id, Colour message)
    {
        // Send the packet to all clients in the lobby
        if (m_Clients.TryGetValue(id, out _) == false) return;
        switch (message.m_Lobby)
        {
            case 1:
                foreach (var client in m_Clients.Values)
                    if (client.m_inLobby1 && client.m_id != id && m_Clients.TryGetValue(id, out _))
                        client.TcpSendPacket(message, true);
                break;

            case 2:
                foreach (var client in m_Clients.Values)
                    if (client.m_inLobby2 && client.m_id != id && m_Clients.TryGetValue(id, out _))
                        client.TcpSendPacket(message, true);
                break;

            case 3:
                foreach (var client in m_Clients.Values)
                    if (client.m_inLobby3 && client.m_id != id && m_Clients.TryGetValue(id, out _))
                        client.TcpSendPacket(message, true);
                break;
        }
    }

    private void TcpHandleLobbySize(int id, LobbySize message)
    {
        if (m_Clients.TryGetValue(id, out _) == false) return;
        m_Clients[id].m_loggedin = true;
        if (message.m_Size != -1)
        {
            switch (message.m_lobby)
            {
                case 1:
                    // Change the lobby size
                    Interlocked.Exchange(ref m_lobby1Size, message.m_Size);

                    // Get the lobby size
                    var lobby1Size = Interlocked.CompareExchange(ref m_lobby1Size, 0, 0);
                    var lobby1SizePacket = new LobbySize(lobby1Size, 1);

                    // Send the lobby size packet to all clients
                    foreach (var client in m_Clients.Values) client.TcpSendPacket(lobby1SizePacket, true);
                    break;

                case 2:
                    Interlocked.Exchange(ref m_lobby2Size, message.m_Size);
                    var lobby2Size = Interlocked.CompareExchange(ref m_lobby2Size, 0, 0);
                    var lobby2SizePacket = new LobbySize(lobby2Size, 2);
                    foreach (var client in m_Clients.Values) client.TcpSendPacket(lobby2SizePacket, true);
                    break;

                case 3:
                    Interlocked.Exchange(ref m_lobby3Size, message.m_Size);
                    var lobby3Size = Interlocked.CompareExchange(ref m_lobby3Size, 0, 0);
                    var lobby3SizePacket = new LobbySize(lobby3Size, 3);
                    foreach (var client in m_Clients.Values) client.TcpSendPacket(lobby3SizePacket, true);
                    break;
            }
        }
        else
        {
            // If the size is -1, reset the lobby size
            var lobby1Size = Interlocked.CompareExchange(ref m_lobby1Size, 0, 0);
            var lobby1SizePacket = new LobbySize(lobby1Size, 1);

            var lobby2Size = Interlocked.CompareExchange(ref m_lobby2Size, 0, 0);
            var lobby2SizePacket = new LobbySize(lobby2Size, 2);

            var lobby3Size = Interlocked.CompareExchange(ref m_lobby3Size, 0, 0);
            var lobby3SizePacket = new LobbySize(lobby3Size, 3);

            // Send the lobby size packet to all clients
            foreach (var client in m_Clients.Values)
            {
                client.TcpSendPacket(lobby1SizePacket, true);
                client.TcpSendPacket(lobby2SizePacket, true);
                client.TcpSendPacket(lobby3SizePacket, true);
            }
        }
    }

    // Figure out some client logic to find out what lobby they are in
    private void TcpHandleInLobby(inLobby message)
    {
        switch (message.m_Lobby)
        {
            case 1:
                m_Clients[message.m_ID].m_inLobby1 = true;
                m_Clients[message.m_ID].m_inLobby2 = false;
                m_Clients[message.m_ID].m_inLobby3 = false;
                break;

            case 2:
                m_Clients[message.m_ID].m_inLobby1 = false;
                m_Clients[message.m_ID].m_inLobby2 = true;
                m_Clients[message.m_ID].m_inLobby3 = false;
                break;

            case 3:
                m_Clients[message.m_ID].m_inLobby1 = false;
                m_Clients[message.m_ID].m_inLobby2 = false;
                m_Clients[message.m_ID].m_inLobby3 = true;
                break;

            case 0:
                m_Clients[message.m_ID].m_inLobby1 = false;
                m_Clients[message.m_ID].m_inLobby2 = false;
                m_Clients[message.m_ID].m_inLobby3 = false;
                break;
        }
    }

    // Start the lobby based on the integer value
    private void TcpHandleStartLobby(startLobby message)
    {
        switch (message.m_Ready)
        {
            // If the lobby is ready, increment the ready count
            // If the lobby is not ready, decrement the ready count
            case true when message.m_Lobby == 1:

                Interlocked.Add(ref m_lobby1ready, 1);
                break;

            case false when message.m_Lobby == 1:
                Interlocked.Add(ref m_lobby1ready, -1);
                break;

            case true when message.m_Lobby == 2:
                Interlocked.Add(ref m_lobby2ready, 1);
                break;

            case false when message.m_Lobby == 2:
                Interlocked.Add(ref m_lobby2ready, -1);
                break;

            case true when message.m_Lobby == 3:
                Interlocked.Add(ref m_lobby3ready, 1);
                break;

            case false when message.m_Lobby == 3:
                Interlocked.Add(ref m_lobby3ready, -1);
                break;
        }

        // If the lobby tries to go below 0, set it to 0
        if (message is { m_Ready: false, m_Lobby: -1 }) Interlocked.Exchange(ref m_lobby1ready, 0);
        if (message is { m_Ready: false, m_Lobby: -2 }) Interlocked.Exchange(ref m_lobby2ready, 0);
        if (message is { m_Ready: false, m_Lobby: -3 }) Interlocked.Exchange(ref m_lobby3ready, 0);

        if (Interlocked.CompareExchange(ref m_lobby1ready, 0, 0) < 0) Interlocked.Exchange(ref m_lobby1ready, 0);

        if (Interlocked.CompareExchange(ref m_lobby2ready, 0, 0) < 0) Interlocked.Exchange(ref m_lobby2ready, 0);

        if (Interlocked.CompareExchange(ref m_lobby3ready, 0, 0) < 0) Interlocked.Exchange(ref m_lobby3ready, 0);

        // If the lobby is ready, send the start lobby packet to all clients in the lobby
        if (Interlocked.CompareExchange(ref m_lobby1ready, 0, 0) == 2)
            foreach (var client in m_Clients.Values)
                if (client.m_inLobby1)
                    client.TcpSendPacket(new startLobby(true, 1), true);

        if (Interlocked.CompareExchange(ref m_lobby2ready, 0, 0) == 2)
            foreach (var client in m_Clients.Values)
                if (client.m_inLobby2)
                    client.TcpSendPacket(new startLobby(true, 2), true);

        if (Interlocked.CompareExchange(ref m_lobby3ready, 0, 0) == 2)
            foreach (var client in m_Clients.Values)
                if (client.m_inLobby3)
                    client.TcpSendPacket(new startLobby(true, 3), true);
    }

    private void TcpHandleClientId(int id, ClientID message)
    {
        // Send the packet to all clients that are not the client who sent the packet
        if (m_Clients.TryGetValue(id, out _) == false) return;
        switch (message.m_lobby)
        {
            case 1:
                foreach (var client in m_Clients.Values)
                    if (client.m_id != id)
                        client.TcpSendPacket(message, false);
                break;

            case 2:
                foreach (var client in m_Clients.Values)
                    if (client.m_id != id)
                        client.TcpSendPacket(message, false);
                break;

            case 3:
                foreach (var client in m_Clients.Values)
                    if (client.m_id != id)
                        client.TcpSendPacket(message, false);
                break;
        }
    }

    #endregion
}