NAT hole punching works by having a server with a known static IP address serve as an intermediary for finding the correct ports of two computers who can both be behind NAT.\
A and B both send UDP packets to the server. Their respective NAT devices Na and Nb create UDP translation states and assign temporary external port numbers EPa and EPb.\
Server examines the packets to find these external port numbers EPa and EPb. Server knows the external NAT IPs EIPa and EIPb already.\
Server passes EIPa:EPa to B and EIPb:EPb to A.\
A sends a packet to EIPb:EPb. NAT A examines the packet and creates a tuple in its translation table: (Source-IP-A, EPa, EIPb, EPb).\
Vice versa for B and its NAT.\
Say that A's packet to B arrives before B's NAT processed B's message to A. This is a worst case scenario.\
A's first packet to B is dropped, because the NAT of B hasn't made the table entry yet.\
B's packet to A is sent, and NAT B makes the translation table entry.\
NAT A receives the packet, and already has a translation table tuple to forward the packet to A.\
Now both NATs have a translation table entry, and every packet after the initial one can be forwarded; A and B can directly communicate.\

If both NATs are restricted cone or symmetric NATs, EPa and EPb to the server will not be valid for direct communication between A and B.\
Some routers pick ports sequentially, in which case a guess can be made to find the direct communication EPa and EPb, because the ports will be adjacent to the EPa and EPb used to contact S.\
However, if only one NAT is restricted cone or symmetric the connection can still be established.\
Say A is restricted. The EPa that Server receives will not be usable for B to send packets to A.\
However, if B is not restricted, A can send packets to B, and B can then find out EPa directly by itself, without the server, and establish direct communications.\

Source: https://en.wikipedia.org/wiki/UDP_hole_punching\

TODO: implement this NAT punching stuff.\

The ForgeNatHolePunch script should be the openly accessible rendezvous server that allows clients to connect to it, in order to then connect to each other.\
Which means that everyone who wants their game to utilize NAT hole punching should modify this script and run it on a raspberry pi or an EC2 instance.\
Or perhaps not modify this server, but include some kind of key, and code the public IP of this server into the game.\
Note that this makes this NAT punching rendezvous server a single point of failure for a large part of the playerbase.\
A DDoS here could take out multiplayer even for private lobbies of friends.\


We need this server to: wait for messages that contain info of who to connect to who. \
-----Target info is contained in the message content, and info about the sender and its NAT is contained in the packet header probably. Not sure how to extract that yet.\
We need this server to then: create and send messages that relay info about the computers and their NATs to one another.\
We then need the clients to: attempt to establish the conversation.\
We can then leave it at that (naive server), or confirm success to the server (not sure if this has a use.)\


In terms of Alloy, we need a few types of messages and interpreters.\
We need a shared message type that can be sent by client and received by rendezvous server, and vice versa.\
This message will have different contents in both directions, so we can write interpreters for this message that do different things in the game client and in the rendezvous.\
This breaks Alloy's 1-to-1 relation principle, but in this case that doesn't really matter in my opinion.\
Then we need a different message and interpreter pair to connect two game instances to each other (client-host or client-server.)\