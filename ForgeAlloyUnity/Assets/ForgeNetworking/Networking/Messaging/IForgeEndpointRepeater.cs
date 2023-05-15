using Forge.Networking.Messaging;
using System;

namespace Forge.Networking.Messaging
{
    public interface IForgeEndpointRepeater
    {
        IMessageReceiptSignature GetNewMessageReceipt();
        ushort ProcessReliableSignature(int id);
        void AddPing(int ms);
        int Ping { get; }
    }
}