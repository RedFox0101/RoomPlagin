using System;
using System.Collections.Generic;
using DarkRift;

public class Match
{
    private List<Player> _players;
    private int _maxCountPlayers;
    private int _readyClient=0;
    private int _round;

    public int CountPlayers => _players.Count;
    public string Id;

    public Match(Player player)
    {
        _maxCountPlayers = 2;
        _players = new List<Player>();
        Id = GenerateId();
        _players = new List<Player>();
        _players.Add(player);
    }

    public void AddReady()
    {
        _readyClient++;
        Console.WriteLine(_readyClient);
        if (_readyClient >= _maxCountPlayers)
        {
            StartGame();
        }
    }

    public void AddPlayer(Player player)
    {
        //player.Client.MessageReceived += MessageReceived;
        _players.Add(player);
    }

    private string GenerateId()
    {
        Random rnd = new Random();
        string id = "";
        for (int i = 0; i < 5; i++)
        {
            id += rnd.Next(0, 9).ToString();
        }
        Console.Write(id);
        return id;
    }

    #region Server
    private void StartGame()
    {
        Console.WriteLine($"Star game in room {Id}");
        using (DarkRiftWriter newPlayerWriter = DarkRiftWriter.Create())
        {
            newPlayerWriter.Write("Vlad");
            newPlayerWriter.Write("RedFox");

            using (Message newPlayerMessage = Message.Create((ushort)Tags.RequestToClient.StartGame, newPlayerWriter))
            {
                foreach (var player in _players)
                {
                    player.Client.SendMessage(newPlayerMessage, SendMode.Reliable);
                }
            }
        }
        SendCards();
        StarNewRound();
    }

    private void SendCards()
    {
        using (DarkRiftWriter newPlayerWriter = DarkRiftWriter.Create())
        {
            newPlayerWriter.Write(0);
            newPlayerWriter.Write(1);
            newPlayerWriter.Write(2);

            using (Message newPlayerMessage = Message.Create((ushort)Tags.RequestToClient.SendCards, newPlayerWriter))
            {
                foreach (var player in _players)
                {
                    player.Client.SendMessage(newPlayerMessage, SendMode.Reliable);
                }
            }
        }
    }

    private void StarNewRound()
    {
        using (DarkRiftWriter newPlayerWriter = DarkRiftWriter.Create())
        {
            newPlayerWriter.Write(_round);
            _round++;
            using (Message newPlayerMessage = Message.Create((ushort)Tags.RequestToClient.StartNewRound, newPlayerWriter))
            {
                foreach (var player in _players)
                {
                    player.Client.SendMessage(newPlayerMessage, SendMode.Reliable);
                }
            }
        }
    }
    #endregion
}

