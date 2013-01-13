﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Text;
using Torrent.Client.Bencoding;

namespace Torrent.Client
{
    /// <summary>
    /// Provides a container class for the peer data contained in the tracker's response.
    /// </summary>
    public class PeerEndpoint
    {
        /// <summary>
        /// Contains the ID of the peer.
        /// </summary>
        /// <remarks>Optional.</remarks>
        public string PeerID { get; private set; }

        /// <summary>
        /// Contains the IP of the peer.
        /// </summary>
        public IPAddress IP { get; private set; }

        /// <summary>
        /// Contains the port number of the peer.
        /// </summary>
        public short Port { get; private set; }

        /// <summary>
        /// Initializes a new instance of the Torrent.Client.PeerEndpoint class.
        /// </summary>
        /// <param name="ip">The IP address of the peer.</param>
        /// <param name="port">The port number of the peer.</param>
        /// <param name="id">The ID of the peer.</param>
        public PeerEndpoint(IPAddress ip, short port, string id="")
        {
            Contract.Requires(ip != null);
            Contract.Requires(port >= 0);
            Contract.Requires(id != null);

            this.PeerID = id;
            this.Port = port;
            this.IP = ip;
        }

        /// <summary>
        /// Initializes a new instance of the Torrent.Client.PeerEndpoint class via a BencodedDictionary.
        /// </summary>
        /// <param name="peer">A BencodedDictionary containing the peer's info.</param>
        public PeerEndpoint(BencodedDictionary peer):
            this(new IPAddress(Encoding.ASCII.GetBytes((BencodedString)peer["ip"])),
            (short)(BencodedInteger)peer["port"],
            (BencodedString)peer["peer id"])
        {  }

        /// <summary>
        /// Initializes a new instance of the Torrent.Client.PeerEndpoint class via a binary data.
        /// </summary>
        /// <param name="peer">Binary data containing the peer's info</param>
        public PeerEndpoint(Byte[] peer):
            this(new IPAddress(peer.Take(4).ToArray()),
            BitConverter.ToInt16(peer, 4))
        {  }
    }
}