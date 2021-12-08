using System;
using System.Collections.Generic;
using Forge.Networking;

namespace ForgeNatHolePunch
{
	public class ForgeNatHolePunch
	{
		//NAT hole punching works by having a server with a known static IP address serve as an intermediary for finding the correct ports of two computers who can both be behind NAT.
		//A and B both send UDP packets to the server. Their respective NAT devices Na and Nb create UDP translation states and assign temporary external port numbers EPa and EPb.
		//Server examines the packets to find these external port numbers EPa and EPb. Server knows the external NAT IPs EIPa and EIPb already.
		//Server passes EIPa:EPa to B and EIPb:EPb to A.
		//A sends a packet to EIPb:EPb. NAT A examines the packet and creates a tuple in its translation table: (Source-IP-A, EPa, EIPb, EPb).
		//Vice versa for B and its NAT.
		//Say that A's packet to B arrives before B's NAT processed B's message to A. This is a worst case scenario.
		//A's first packet to B is dropped, because the NAT of B hasn't made the table entry yet.
		//B's packet to A is sent, and NAT B makes the translation table entry.
		//NAT A receives the packet, and already has a translation table tuple to forward the packet to A.
		//Now both NATs have a translation table entry, and every packet after the initial one can be forwarded; A and B can directly communicate.

		//If both NATs are restricted cone or symmetric NATs, EPa and EPb to the server will not be valid for direct communication between A and B.
		//Some routers pick ports sequentially, in which case a guess can be made to find the direct communication EPa and EPb, because the ports will be adjacent to the EPa and EPb used to contact S.
		//However, if only one NAT is restricted cone or symmetric the connection can still be established.
		//Say A is restricted. The EPa that Server receives will not be usable for B to send packets to A.
		//However, if B is not restricted, A can send packets to B, and B can then find out EPa directly by itself, without the server, and establish direct communications.

		//TODO: implement this NAT punching stuff.

		static void Main(string[] args)
		{

		}
	}
}
