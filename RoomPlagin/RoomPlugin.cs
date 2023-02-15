using System;
using System.Collections.Generic;
using System.Linq;
using DarkRift;
using DarkRift.Server;

namespace RoomPlagin
{
    public class RoomPlugin : Plugin
    {
        private List<Match> _rooms;
        private Dictionary<int, Player> _clients;

        public override bool ThreadSafe => false;

        public override Version Version => new Version(1, 0, 0);

        public RoomPlugin(PluginLoadData loadData) : base(loadData)
        {
            _clients = new Dictionary<int, Player>();
            _rooms = new List<Match>();
            ClientManager.ClientConnected += ClientManager_ClientConnected;
            ClientManager.ClientDisconnected += ClientManager_ClientDisconnected;
        }

        private void ClientManager_ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            _clients.Remove(e.Client.ID);
        }

        private void ClientManager_ClientConnected(object sender, ClientConnectedEventArgs e)
        {
            var newPlayer = new Player(e.Client);
            _clients.Add(e.Client.ID, newPlayer);
            Console.WriteLine($"Count Client {_clients.Count}");
            e.Client.MessageReceived += Client_MessageReceived;
        }

        private void Client_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            try
            {
                using (Message message = e.GetMessage())
                using (DarkRiftReader reader = message.GetReader())
                {
                    switch (message.Tag)
                    {
                        case 0:
                            Console.WriteLine(reader.ReadString());
                            CreateRoom(_clients[e.Client.ID]);
                            break;
                        case (ushort)Tags.ResponesFromClient.Join:
                            FindRoom(reader.ReadString(), _clients[e.Client.ID]);
                            break;
                        case (ushort)Tags.ResponesFromClient.Ready:
                            Console.WriteLine("Ready");
                           var roomFind= _rooms.First(room => room.Id == reader.ReadString());
                            roomFind.AddReady();
                            break;
                    }
                }
            }
            catch
            {
                // Do disconnect/kick maybe later if they do be acting up.
            }
        }

        private void CreateRoom(Player player)
        {
            var newRoom = new Match(player);
            _rooms.Add(newRoom);
            player.RoomId = newRoom.Id;
            Join(player, newRoom);
        }

        private void FindRoom(string Id, Player player)
        {
            Match roomFind = _rooms.FirstOrDefault(room => room.Id == Id);
            Join(player, roomFind);
            roomFind.AddPlayer(player);
        }

        private static void Join(Player player, Match newRoom)
        {
            using (DarkRiftWriter newPlayerWriter = DarkRiftWriter.Create())
            {
                newPlayerWriter.Write(newRoom.Id);

                using (Message newPlayerMessage = Message.Create((ushort)Tags.RequestToClient.Joined, newPlayerWriter))
                {
                    player.Client.SendMessage(newPlayerMessage, SendMode.Reliable);
                }
            }
        }
    }
}

public static class Tags
{
    public enum RequestToClient { Joined = 1,  StartGame=3, SendCards=4, StartNewRound=6}
    public enum ResponesFromClient { CreateRoom = 0, Join = 2 , Ready=5}
}

