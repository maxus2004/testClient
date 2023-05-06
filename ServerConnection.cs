using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

class ServerConnection
{
    TcpClient tcpClient;
    UdpClient udp;
    NetworkStream tcp;

    bool connected = false;

    public ServerConnection(string ip)
    {
        tcpClient = new TcpClient(ip, 42069);
        tcp = tcpClient.GetStream();
        udp = new UdpClient(ip, 42069);
        startReceiveLoops();
    }

    public void send(Packet packet)
    {
        if (packet.protocol == Packet.Protocol.TCP)
        {
            tcp.Write(packet.bytes);
        }
        else
        {
            udp.Send(packet.bytes);
        }
    }

    void receive(Packet packet)
    {
        switch (packet.type)
        {
            case Packet.Type.PING:
                if(connected==false){
                    connected = true;
                    send(new Packet(Packet.Type.GET_ID, 0));
                }
                Console.WriteLine("PONG");
                break;
            case Packet.Type.GET_ID:
                Console.WriteLine("my id: " + BitConverter.ToInt32(packet.bytes, 4));
                Program.id = BitConverter.ToInt32(packet.bytes, 4);
                break;
            case Packet.Type.WON:
                Console.WriteLine("player " + BitConverter.ToInt32(packet.bytes, 4) + " won");
                break;
            case Packet.Type.LOST:
                Console.WriteLine("player " + BitConverter.ToInt32(packet.bytes, 4) + " lost");
                break;
            case Packet.Type.JOIN:
                //only for server
                break;
            case Packet.Type.JOIN_FAILED:
                Console.WriteLine("lobby is full");
                break;
            case Packet.Type.JOINED:
                Console.WriteLine("player " + BitConverter.ToInt32(packet.bytes, 4) + " joined");
                break;
            case Packet.Type.CREATE_LOBBY:
                //only for server
                break;
            case Packet.Type.CREATED_LOBBY:
                Console.WriteLine("new lobby id: " + BitConverter.ToInt32(packet.bytes, 8));
                break;
            case Packet.Type.LEFT:
                Console.WriteLine("player " + BitConverter.ToInt32(packet.bytes, 4) + " left");
                break;
            case Packet.Type.MOVED:
                Console.WriteLine("player " + BitConverter.ToInt32(packet.bytes, 4) + " moved");
                break;
        }
    }

    void startReceiveLoops()
    {
        new Thread(TcpLoop).Start();
        new Thread(UdpLoop).Start();
    }

    void TcpLoop()
    {
        try
        {
            while (tcpClient.Connected)
            {
                byte[] typeBuffer = new byte[4];
                tcp.Read(typeBuffer, 0, 4);
                Packet.Type type = (Packet.Type)BitConverter.ToInt32(typeBuffer);
                byte[] bytes = new byte[Packet.Lengths[type]];
                typeBuffer.CopyTo(bytes,0);
                int read = tcp.Read(bytes, 4, Packet.Lengths[type]-4);
                if (read != Packet.Lengths[type]-4)
                {
                    tcpClient.Close();
                    break;
                }
                receive(new Packet(Packet.Protocol.TCP, bytes));
            }
        }
        catch (Exception e)
        {
            tcpClient.Close();
            Console.Error.WriteLine(e);
        }
        tcpClient.Close();
        udp.Close();
    }
    void UdpLoop()
    {
        try
        {
            while (tcpClient.Connected)
            {
                IPEndPoint ip = new IPEndPoint(IPAddress.Any, 0);
                byte[] bytes = udp.Receive(ref ip);
                receive(new Packet(Packet.Protocol.UDP, bytes));
            }
        }
        catch (Exception e)
        {
            udp.Close();
            Console.Error.WriteLine(e);
        }
    }
}