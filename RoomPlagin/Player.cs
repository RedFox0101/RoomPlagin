using DarkRift.Server;

public class Player
{
    public IClient Client;
    public string RoomId;

    public Player(IClient client)
    {
        Client = client;
    }
}

