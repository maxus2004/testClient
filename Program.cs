class Program
{

    static ServerConnection serverConnection = new ServerConnection("109.206.148.223");

    public static int id = 0;

    static void Main()
    {
        while(id==0)
        {
            serverConnection.send(new Packet(Packet.Type.PING, id));
            Thread.Sleep(50);
        }

        Console.WriteLine("connected");

        while (true)
        {
            switch (Console.ReadLine())
            {
                case "ping":
                    serverConnection.send(new Packet(Packet.Type.PING, id));
                    break;
                case "get id":
                    serverConnection.send(new Packet(Packet.Type.GET_ID, id));
                    break;
                case "win":
                    serverConnection.send(new Packet(Packet.Type.WON, id));
                    break;
                case "loose":
                    serverConnection.send(new Packet(Packet.Type.LOST, id));
                    break;
                case "join":
                    Console.Write("enter lobby id: ");
                    int lobby = int.Parse(Console.ReadLine());
                    serverConnection.send(new Packet(Packet.Type.JOIN, id, lobby));
                    break;
                case "create lobby":
                    serverConnection.send(new Packet(Packet.Type.CREATE_LOBBY, id));
                    break;
                case "leave":
                    serverConnection.send(new Packet(Packet.Type.LEFT, id));
                    break;
                case "move":
                    byte[] bytes = new byte[4 * 4];
                    serverConnection.send(new Packet(Packet.Type.MOVED, id, bytes));
                    break;
            }
        }
    }
}