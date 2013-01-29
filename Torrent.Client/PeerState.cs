﻿using System.Collections;
using System.Net;
using System.Net.Sockets;

namespace Torrent.Client
{
    public class PeerState
    {
        public PeerState(Socket socket, IPEndPoint endpoint)
        {
            Socket = socket;
            EndPoint = endpoint;
            AmChoked = true;
            IsChoked = true;
        }

        public bool AmChoked { get; set; }
        public bool AmInterested { get; set; }
        public bool IsChoked { get; set; }
        public bool IsInterested { get; set; }
        public Socket Socket { get; set; }
        public bool ReceivedHandshake { get; set; }
        public bool SentHandshake { get; set; }
        public IPEndPoint EndPoint { get; set; }
        public string ID { get; set; }
        public BitArray Bitfield { get; set; }

        public override string ToString()
        {
            return string.Format("{0}, sent: {1}, received: {2}", EndPoint, SentHandshake, ReceivedHandshake);
        }
    }
}