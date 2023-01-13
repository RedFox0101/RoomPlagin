using System;
using System.Collections.Generic;
using System.Diagnostics;
using DarkRift;
using DarkRift.Server;
using System.Windows.Forms;

namespace RoomPlagin
{
    public class RoomPlugin:Plugin
    {
        public override bool ThreadSafe => false;

        private Queue<IClient> _createRoomQueue = new Queue<IClient>();
        private string _pathServer;
        public override Version Version => new Version(1, 0, 0);

        public RoomPlugin(PluginLoadData loadData) : base(loadData)
        {
            OpenFileDialog OD = new OpenFileDialog();
            OD.DefaultExt = ".exe";
            OD.Filter = "Program file (.exe)|*.exe";
            if (OD.ShowDialog() == DialogResult.OK)
            {
                Console.WriteLine(OD.FileName);
                _pathServer = OD.FileName;
            }
            Console.ReadKey(true);
            ClientManager.ClientConnected += ClientManager_ClientConnected;
            ClientManager.ClientDisconnected += ClientManager_ClientDisconnected;
        }

        private void ClientManager_ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
        }

        private void ClientManager_ClientConnected(object sender, ClientConnectedEventArgs e)
        {
            e.Client.MessageReceived += Client_MessageReceived;
        }

        private void Client_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            try
            {
                using (DarkRift.Message message = e.GetMessage())
                using (DarkRiftReader reader = message.GetReader())
                {
                    OpCodes opCode = (OpCodes)message.Tag;

                    switch (opCode)
                    {
                        case OpCodes.CreateRoom:
                            if (!_createRoomQueue.Contains(e.Client))
                            {
                                _createRoomQueue.Enqueue(e.Client);
                                Console.WriteLine("Create Room");
                                Process.Start(@_pathServer);
                            }
                            break;
                        case OpCodes.OnCreateRoom:
                            ushort roomId = reader.ReadUInt16();
                            string roomName = reader.ReadString();
                            Console.WriteLine($"Room ID {roomId}");
                            Console.WriteLine($"Room name {roomName}");

                            using (DarkRiftWriter writer = DarkRiftWriter.Create())
                            {
                                writer.Write(roomId);
                                writer.Write(roomName);
                                Console.WriteLine($"Create Room ID{roomId}");
                                using (DarkRift.Message message1 = DarkRift.Message.Create((ushort)OpCodes.RequestCreatedRoom, writer))
                                {
                                    _createRoomQueue.Dequeue().SendMessage(message1, SendMode.Reliable);
                                }

                            }
                            break;
                    }
                }
            }
            catch
            {
                // Do disconnect/kick maybe later if they do be acting up.
            }
        }
    }

    enum OpCodes { CreateRoom = 20, OnCreateRoom = 21, RequestCreatedRoom = 22 }
}
