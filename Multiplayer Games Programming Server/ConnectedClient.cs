#region Includes

// Includes
using Multiplayer_Games_Programming_Packet_Library;
using System.Diagnostics;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace Multiplayer_Games_Programming_Server;

#endregion

internal class ConnectedClient
{
    #region Memeber Varibles

    // Network Streams
    private readonly StreamReader m_reader;

    private readonly StreamWriter m_writer;
    private readonly NetworkStream m_stream;
    public Socket m_socket;
    public UdpReceiveResult m_result;

    // RSA Encryption
    private RSACryptoServiceProvider m_rsaProvider = null!;

    public RSAParameters m_publicKey;
    public RSAParameters m_serverPrivateKey;

    // Client Data
    public int m_heartbeatTimer = 0;

    public int m_id;
    public bool m_inLobby1 = false;
    public bool m_inLobby2 = false;
    public bool m_inLobby3 = false;
    public bool m_loggedin = false;

    #endregion

    #region Constructor

    public ConnectedClient(Socket socket)
    {
        m_stream = new NetworkStream(socket, false);
        m_reader = new StreamReader(m_stream, Encoding.UTF8);
        m_writer = new StreamWriter(m_stream, Encoding.UTF8);
        m_socket = socket;
    }

    // Close the connection
    public void Close()
    {
        try
        {
            m_socket.Disconnect(false);
            m_socket.Close();
            m_stream.Close();
            m_reader.Close();
            m_writer.Close();
            m_socket.Dispose();
        }
        catch (SocketException ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }

    #endregion

    #region Read and Send TCP/UDP

    // Read the message
    public Packet? Read()
    {
        try
        {
            // Read the message to a JSON string
            var json = m_reader.ReadLine();

            // Deserialize the JSON string to a packet
            return Packet.Deserialize(json);
        }
        catch (SocketException ex)
        {
            Debug.WriteLine(ex.Message);
            return null;
        }
    }

    public void TcpSendPacket(Packet message, bool sendEncrypted)
    {
        try
        {
            lock (this) // Lock the client
            {
                var sentMessage = message.ToJson(); // Convert the message to json

                // Encrypt the message if needed
                if (sendEncrypted) sentMessage = new EncryptedPacket(m_id, Encrypted(message)).ToJson();

                try
                {
                    // Write the message to the stream
                    m_writer.WriteLine(sentMessage);

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

    public void UdpSendPacket(Packet message, UdpClient client, bool sendEncrypted)
    {
        try
        {
            lock (this) // Lock the client
            {
                var sentMessage = message.ToJson(); // Convert the message to json

                // Encrypt the message if needed
                if (sendEncrypted) sentMessage = new EncryptedPacket(m_id, Encrypted(message)).ToJson();

                // Convert the message to bytes
                var bytes = Encoding.UTF8.GetBytes(sentMessage);

                // Send the message to the client
                client.Send(bytes, bytes.Length, m_result.RemoteEndPoint);
                Debug.WriteLine("UDP Message Sent: " + sentMessage);
            }
        }
        catch (SocketException ex)
        {
            Debug.WriteLine("Client UDP Send Method exception: " + ex.Message);
        }
    }

    #endregion

    #region Encyption and Decryption

    // Encrypt the message
    public byte[] Encrypted(Packet message)
    {
        // Lock the RSA
        lock (m_rsaProvider)
        {
            // Import the clients public key
            m_rsaProvider.ImportParameters(m_publicKey);

            // Convert the message to json
            var json = message.ToJson();

            // Encrypt the message using the clients public key
            var encrypted = m_rsaProvider.Encrypt(Encoding.UTF8.GetBytes(json), false);
            return encrypted;
        }
    }

    // Decrypt the message
    public Packet? Decrypt(byte[] encrypted)
    {
        // Lock the RSA
        lock (m_rsaProvider)
        {
            // Import the servers private key
            m_rsaProvider.ImportParameters(m_serverPrivateKey);

            // Decrypt the message using the servers private key
            var decrypted = m_rsaProvider.Decrypt(encrypted, false);

            // Convert the decrypted message to json
            var json = Encoding.UTF8.GetString(decrypted);

            // Deserialize the json to a packet
            var packet = Packet.Deserialize(json);
            return packet;
        }
    }

    // Generate the clients keys
    public void GenerateKeys(RSAParameters serverPublicKey, RSAParameters serverPrivateKey)
    {
        m_rsaProvider = new RSACryptoServiceProvider(2048); // Create a new RSA provider

        // Lock the RSA
        lock (m_rsaProvider)
        {
            // Export the clients public key
            m_publicKey = m_rsaProvider.ExportParameters(false);

            // Export the clients private key
            var privateKey = m_rsaProvider.ExportParameters(true);

            // Save the server public key and the clients private key
            m_serverPrivateKey = serverPrivateKey;
            Packet keys = new ClientID(m_id, m_publicKey, privateKey,
                serverPublicKey); // Send the client keys and the server public key, but forget the clients private key
            TcpSendPacket(keys, false);
        }
    }

    #endregion
}