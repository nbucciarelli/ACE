using System;
using System.IO;

namespace ACE.Server.Network
{
    public class ClientMessage
    {
        public BinaryReader Payload { get; }

        public MemoryStream Data { get; }

        private byte[] _Data { get; } // allows copying the message without side-effects.

        public int Length { get { return _Data.Length; } }

        public uint Opcode { get; }

        //public ClientMessage(MemoryStream stream)
        //{
        //    Data = stream;
        //    Payload = new BinaryReader(Data);
        //    Opcode = Payload.ReadUInt32();
        //}

        public ClientMessage(MemoryStream stream)
        {
            var reader = new BinaryReader(stream);
            byte[] data = reader.ReadBytes((int)stream.Length);
            _Data = data;
            Data = new MemoryStream(data);
            Payload = new BinaryReader(Data);
            Opcode = Payload.ReadUInt32();
        }

        public ClientMessage(byte[] data)
        {
            _Data = data;
            Data = new MemoryStream(data);
            Payload = new BinaryReader(Data);
            Opcode = Payload.ReadUInt32();
        }

        public ClientMessage Clone()
        {
            // allows multiple consumers without side-effects.
            var clone = new ClientMessage(_Data);
            //Console.WriteLine($"ClientMessage.Clone() -- Original: {_Data.Length}; New: {clone._Data.Length}");
            return clone;
        }
    }
}
