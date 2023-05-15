using System.Collections.Generic;
using Forge.DataStructures;
using Forge.Factory;

namespace Forge.Networking.Messaging
{
    public class ForgeEndpointRepeater : IForgeEndpointRepeater
    {
        public ISignatureGenerator<int> _generator;

        // Packets that have not been received
        private HashSet<int> _missingPackets = new HashSet<int>();

        // The most recent packet that has been received
        protected volatile int _lastPacket = -1;

        private RateTracker ping = new RateTracker(100, 50, false);

        public ForgeEndpointRepeater()
        {
            _generator = AbstractFactory.Get<INetworkTypeFactory>().GetNew<ISignatureGenerator<int>>();
        }

        public IMessageReceiptSignature GetNewMessageReceipt()
        {
            var receipt = AbstractFactory.Get<INetworkTypeFactory>().GetNew<IMessageReceiptSignature>();
            receipt.Init(_generator);
            return receipt;
        }

        public ushort ProcessReliableSignature(int id)
        {
            ushort recentPackets = 0;

            lock (_missingPackets)
            {

                if (id > _lastPacket)
                {
                    // Store any missing packets
                    for (int i = (_lastPacket + 1); i < id; i++)
                    {
                        _missingPackets.Add(i);
                    }

                    _lastPacket = id;
                }
                else
                {
                    // Remove from missing packets
                    if (!_missingPackets.Remove(id))
                    {
                        // Duplicate packet
                    }
                }
                for (int i = 1; i <= 16; ++i)
                {
                    if (!this._missingPackets.Contains(id - i))
                    {
                        recentPackets |= (ushort)(1 << (i - 1));
                    }
                }
            }
            return recentPackets;
        }

        public void AddPing(int ms)
        {
            ping.UpdateStats(ms);
        }

        public int Ping { get { return ping.LastAverageValue; } }

    }
}
