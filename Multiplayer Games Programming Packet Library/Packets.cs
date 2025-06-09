#region Includes

// Includes
using Multiplayer_Games_Programming_Packet_Library;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;

#endregion

namespace Multiplayer_Games_Programming_Packet_Library
{
    #region Movement Direction Enum

    // Movement Direction Enum shared between the packet library and the client
    public enum MovementDirection
    {
        Left,
        Right,
        None
    }

    #endregion

    #region Packet Base Class

    public abstract class Packet
    {
        // Packet Type Enum shared between the packet library and the client
        public enum PacketType
        {
            Empty,
            MessagePacket,
            ClientId,
            Position,
            LobbySize,
            InLobby,
            StartLobby,
            Colour,
            Disconnect,
            Heartbeat,
            Encrypted,
            LobbyScore,
            Username,
            Emote,
            ResetGameObjects
        }

        // Base class for all packets
        [JsonPropertyName("Type")] public PacketType m_packetType { get; set; }

        [JsonPropertyName("Name")] public string m_Name { get; set; }

        public virtual void PrintToConsole()
        {
            Console.WriteLine("Name: {0} is a {1}, this is the base class", m_Name, m_packetType);
        }

        // Serialise the packet to a json string
        public string ToJson()
        {
            try
            {
                // Lock the object to prevent multiple threads from accessing it at the same time
                lock (this)
                {
                    // Create a new json serialiser with the packet converter
                    var options = new JsonSerializerOptions
                    {
                        Converters = { new PacketConverter() },
                        IncludeFields = true
                    };
                    // Serialise the packet to a json string using the options
                    return JsonSerializer.Serialize(this, options);
                }
            }
            catch (JsonException ex)
            {
                throw new JsonException(ex.Message);
            }
        }

        public static Packet? Deserialize(string? json)
        {
            try
            {
                if (json == null)
                    return null;

                // Lock the object to prevent multiple threads from accessing it at the same time
                lock (json)
                {
                    // Create a new json deserializer with the packet converter
                    var options = new JsonSerializerOptions
                    {
                        Converters = { new PacketConverter() },
                        IncludeFields = true
                    };

                    // Deserialise the json string to a packet object using the options
                    return JsonSerializer.Deserialize<Packet>(json, options);
                }
            }
            catch (JsonException ex)
            {
                throw new JsonException(ex.Message);
            }
        }
    }

    #endregion
}

#region Packet Converter Options

internal class PacketConverter : JsonConverter<Packet>
{
    // Read the json string and convert it to a packet object
    public override Packet? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        try
        {
            //  Parse the json and try to get the type property
            using var doc = JsonDocument.ParseValue(ref reader);
            var root = doc.RootElement;
            if (!root.TryGetProperty("Type", out var typeProperty)) throw new JsonException("Unknown type");
            if (typeProperty.GetByte() == (byte)Packet.PacketType.Empty)
                return JsonSerializer.Deserialize<EmptyPacket>(root.GetRawText(), options);
            if (typeProperty.GetByte() == (byte)Packet.PacketType.MessagePacket)
                return JsonSerializer.Deserialize<MessagePacket>(root.GetRawText(), options);
            if (typeProperty.GetByte() == (byte)Packet.PacketType.ClientId)
                return JsonSerializer.Deserialize<ClientID>(root.GetRawText(), options);
            if (typeProperty.GetByte() == (byte)Packet.PacketType.Position)
                return JsonSerializer.Deserialize<Position>(root.GetRawText(), options);
            if (typeProperty.GetByte() == (byte)Packet.PacketType.LobbySize)
                return JsonSerializer.Deserialize<LobbySize>(root.GetRawText(), options);
            if (typeProperty.GetByte() == (byte)Packet.PacketType.InLobby)
                return JsonSerializer.Deserialize<inLobby>(root.GetRawText(), options);
            if (typeProperty.GetByte() == (byte)Packet.PacketType.StartLobby)
                return JsonSerializer.Deserialize<startLobby>(root.GetRawText(), options);
            if (typeProperty.GetByte() == (byte)Packet.PacketType.Colour)
                return JsonSerializer.Deserialize<Colour>(root.GetRawText(), options);
            if (typeProperty.GetByte() == (byte)Packet.PacketType.Disconnect)
                return JsonSerializer.Deserialize<Disconnect>(root.GetRawText(), options);
            if (typeProperty.GetByte() == (byte)Packet.PacketType.Heartbeat)
                return JsonSerializer.Deserialize<Heartbeat>(root.GetRawText(), options);
            if (typeProperty.GetByte() == (byte)Packet.PacketType.Encrypted)
                return JsonSerializer.Deserialize<EncryptedPacket>(root.GetRawText(), options);
            if (typeProperty.GetByte() == (byte)Packet.PacketType.LobbyScore)
                return JsonSerializer.Deserialize<LobbyScore>(root.GetRawText(), options);
            if (typeProperty.GetByte() == (byte)Packet.PacketType.Username)
                return JsonSerializer.Deserialize<Username>(root.GetRawText(), options);
            if (typeProperty.GetByte() == (byte)Packet.PacketType.Emote)
                return JsonSerializer.Deserialize<Emote>(root.GetRawText(), options);
            if (typeProperty.GetByte() == (byte)Packet.PacketType.ResetGameObjects)
                return JsonSerializer.Deserialize<ResetGameObjects>(root.GetRawText(), options);
        }
        catch (JsonException ex)
        {
            Debug.WriteLine(ex.Message);
        }

        return null;
    }

    // Write the packet object to a json string
    public override void Write(Utf8JsonWriter writer, Packet value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}

#endregion

#region Packet Types

// Each packet is a class that inherits from the base packet class
// They hold properties that are serialised and deserialize to json
// These properties are sent over the network to communicate between the server and client
// The properties are used to update the game state on the client and server
// I am not going to explain each packet as they are pretty self-explanatory
public class ResetGameObjects : Packet
{
    [JsonPropertyName("ID")] public int m_lobbyID;

    public ResetGameObjects()
    {
        m_packetType = PacketType.ResetGameObjects;
        m_Name = "ResetGameObjects";
    }

    public ResetGameObjects(int lobbyId)
    {
        m_packetType = PacketType.ResetGameObjects;
        m_Name = "ResetGameObjects";
        m_lobbyID = lobbyId;
    }
}

public class Emote : Packet
{
    [JsonPropertyName("Emote")] public int m_emote;
    [JsonPropertyName("ID")] public int m_lobbyID;

    public Emote()
    {
        m_packetType = PacketType.Emote;
        m_Name = "Emote";
    }

    public Emote(int emoteNum, int lobbyId)
    {
        m_packetType = PacketType.Emote;
        m_Name = "Emote";

        m_emote = emoteNum;
        m_lobbyID = lobbyId;
    }
}

public class Username : Packet
{
    [JsonPropertyName("ID")] public int m_lobbyID;
    [JsonPropertyName("Username")] public string m_Username;

    public Username()
    {
        m_packetType = PacketType.Username;
        m_Name = "Username";
    }

    public Username(string username, int lobbyId)
    {
        m_packetType = PacketType.Username;
        m_Name = "Username";
        m_Username = username;
        m_lobbyID = lobbyId;
    }
}

public class LobbyScore : Packet
{
    [JsonPropertyName("ID")] public int m_lobbyID;
    [JsonPropertyName("Score")] public int m_Score;

    public LobbyScore()
    {
        m_packetType = PacketType.LobbyScore;
        m_Name = "LobbyScore";
    }

    public LobbyScore(int score)
    {
        m_packetType = PacketType.LobbyScore;
        m_Name = "LobbyScore";
        m_Score = score;
    }

    public LobbyScore(int score, int lobbyId)
    {
        m_packetType = PacketType.LobbyScore;
        m_Name = "LobbyScore";
        m_Score = score;
        m_lobbyID = lobbyId;
    }
}

// This packet acts as a wrapper for the encrypted data
public class EncryptedPacket : Packet
{
    [JsonPropertyName("Encryption")] public byte[]? m_Encryption;

    [JsonPropertyName("ID")] public int m_id;

    public EncryptedPacket()
    {
        m_packetType = PacketType.Encrypted;
        m_Encryption = Array.Empty<byte>();
        m_id = -1;
        m_Name = "Encrypted";
    }

    public EncryptedPacket(int id, byte[]? encryption)
    {
        m_id = id;
        m_packetType = PacketType.Encrypted;
        m_Encryption = encryption;
        m_Name = "Encrypted";
    }
}

public class MessagePacket : Packet
{
    [JsonPropertyName("ID")] public int m_Id = -1;
    [JsonPropertyName("Message")] public string m_Message;

    public MessagePacket()
    {
        m_packetType = PacketType.MessagePacket;
        m_Name = "Message";
    }

    public MessagePacket(string message)
    {
        m_packetType = PacketType.MessagePacket;
        m_Name = "Message";
        m_Message = message;
    }

    public MessagePacket(string message, int id)
    {
        m_packetType = PacketType.MessagePacket;
        m_Name = "Message";
        m_Message = message;
        m_Id = id;
    }

    public override void PrintToConsole()
    {
        Console.WriteLine("{0} is a {1}. Their Message is {2}", m_Name, m_packetType, m_Message);
    }
}

public class Disconnect : Packet
{
    public Disconnect()
    {
        m_packetType = PacketType.Disconnect;
        m_Name = "Disconnect";
    }
}

// Heartbeat packet used to check if the client is still connected
public class Heartbeat : Packet
{
    [JsonPropertyName("ID")] public int m_ID;

    public Heartbeat()
    {
        m_packetType = PacketType.Heartbeat;
        m_Name = "Heartbeat";
    }

    public Heartbeat(int id)
    {
        m_packetType = PacketType.Heartbeat;
        m_Name = "Heartbeat";
        m_ID = id;
    }
}

// Client ID packet used to assign a unique ID to the client and keys for encryption
public class ClientID : Packet
{
    [JsonPropertyName("ID")] public int m_ID;

    [JsonPropertyName("Lobby")] public int m_lobby;

    [JsonPropertyName("Private Key")] public RSAParameters m_privateKey;

    [JsonPropertyName("Public Key")] public RSAParameters m_publicKey;

    [JsonPropertyName("Server Public Key")] public RSAParameters m_serverPublicKey;

    public ClientID()
    {
        m_packetType = PacketType.ClientId;
        m_Name = "CLIENTID";
    }

    public ClientID(int ID, RSAParameters publicKey, RSAParameters privateKey, RSAParameters serverPublicKey)
    {
        m_packetType = PacketType.ClientId;
        m_Name = "CLIENTID";
        m_ID = ID;
        m_publicKey = publicKey;
        m_privateKey = privateKey;
        m_serverPublicKey = serverPublicKey;
    }

    public ClientID(int ID, int lobby)
    {
        m_packetType = PacketType.ClientId;
        m_Name = "CLIENTID";
        m_ID = ID;
        m_lobby = lobby;
    }
}

public class LobbySize : Packet
{
    [JsonPropertyName("Lobby")] public int m_lobby;

    [JsonPropertyName("Size")] public int m_Size;

    public LobbySize()
    {
        m_packetType = PacketType.LobbySize;
        m_Name = "LOBBY1SIZE";
    }

    public LobbySize(int size, int lobby)
    {
        m_packetType = PacketType.LobbySize;
        m_Name = "LOBBY1SIZE";
        m_Size = size;
        m_lobby = lobby;
    }
}

// Position packet used to update the position of the player on the server,
// also send the player direction so that some prediction can be done
public class Position : Packet
{
    [JsonPropertyName("Direction")] public MovementDirection m_direction;
    [JsonPropertyName("ID")] public int m_ID;

    [JsonPropertyName("Lobby")] public int m_Lobby;

    [JsonPropertyName("XPOS")] public float m_XPOS;

    [JsonPropertyName("YPOS")] public float m_YPOS;

    public Position()
    {
        m_packetType = PacketType.Position;
        m_Name = "POSITION";
    }

    public Position(int id, float x, float y, MovementDirection direction, int lobby)
    {
        m_packetType = PacketType.Position;
        m_Name = "POSITION";

        m_ID = id;
        m_XPOS = x;
        m_YPOS = y;
        m_direction = direction;
        m_Lobby = lobby;
    }
}

public class Colour : Packet
{
    [JsonPropertyName("COLOUR A")] public float m_Colour_A;
    [JsonPropertyName("COLOUR B")] public float m_Colour_B;
    [JsonPropertyName("COLOUR G")] public float m_Colour_G;

    [JsonPropertyName("COLOUR R")] public float m_Colour_R;
    [JsonPropertyName("ID")] public int m_ID;
    [JsonPropertyName("Lobby")] public int m_Lobby;

    public Colour()
    {
        m_packetType = PacketType.Colour;
        m_Name = "COLOUR";
    }

    public Colour(int id, float r, float g, float b, float a, int lobby)
    {
        m_packetType = PacketType.Colour;
        m_Name = "COLOUR";

        m_ID = id;
        m_Colour_R = r;
        m_Colour_G = g;
        m_Colour_B = b;
        m_Colour_A = a;
        m_Lobby = lobby;
    }
}

public class inLobby : Packet
{
    [JsonPropertyName("ID")] public int m_ID;

    [JsonPropertyName("Lobby")] public int m_Lobby;

    public inLobby()
    {
        m_packetType = PacketType.InLobby;
        m_Name = "INLOBBY";
    }

    public inLobby(int ID, int lobby)
    {
        m_packetType = PacketType.InLobby;
        m_Name = "INLOBBY";

        m_ID = ID;
        m_Lobby = lobby;
    }
}

public class startLobby : Packet
{
    [JsonPropertyName("Lobby")] public int m_Lobby;
    [JsonPropertyName("Ready")] public bool m_Ready;

    public startLobby()
    {
        m_packetType = PacketType.StartLobby;
        m_Name = "STARTLOBBY";
    }

    public startLobby(bool ready, int lobby)
    {
        m_packetType = PacketType.StartLobby;
        m_Name = "STARTLOBBY";

        m_Ready = ready;
        m_Lobby = lobby;
    }
}

public class EmptyPacket : Packet
{
    public EmptyPacket()
    {
        m_packetType = PacketType.Empty;
        m_Name = "EMPTY";
    }

    public override void PrintToConsole()
    {
        Console.WriteLine("{0} is a {1}.", m_Name, m_packetType);
    }
}

#endregion