using System;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using Rug.Osc;
using Newtonsoft.Json;

namespace InsLab.Muse
{
    public class Muse
    {
        private OscReceiver receiver;
        private Thread readThread = null;
        private bool reading = false;
        private readonly char[] seperator = new char[] { '/' };
        private Dictionary<MuseData, List<IMusePacket>> data;

        public OscSocketState State { get => receiver.State; }
        public string Model { get; } = "Muse 2016 (MU-02)";
        public string Name { get; }
        public MuseData DataToRead { get; set; } = MuseData.EEG;

        public Muse(int port = 7000)
        {
            receiver = new OscReceiver(port);

            receiver.Connect();
            var packet = receiver.Receive();
            var message = (OscMessage)packet;
            Name = message.Address.Split(seperator, 2)[0];
        }

        public void StartReading()
        {
            if (readThread != null && readThread.IsAlive)
            {
                return;
            }

            reading = true;
            if (receiver.State != OscSocketState.Connected)
            {
                receiver.Connect();
            }
            readThread = new Thread(ListenLoop);
            readThread.Start();
        }

        public void StopReading()
        {
            reading = false;
            if (receiver.State != OscSocketState.Closed)
            {
                receiver.Close();
            }
            readThread?.Join();
        }

        public string ConvertDataToJson()
        {
            var jsonString = JsonConvert.SerializeObject(data);
            return jsonString;
        }

        protected void ListenLoop()
        {
            data = new Dictionary<MuseData, List<IMusePacket>>();

            foreach (var museData in MuseDataHelper.GetValues())
            {
                if (DataToRead.HasFlag(museData))
                {
                    data[museData] = new List<IMusePacket>();
                }
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            while (receiver.State == OscSocketState.Connected)
            {
                // get the next message 
                // this will block until one arrives or the socket is closed
                OscPacket packet;
                try
                {
                    packet = receiver.Receive();
                }
                catch (Exception)
                {
                    // ignore
                    continue;
                }
                OscMessage message = (OscMessage)packet;

                var packetReceivedTickCount = stopwatch.ElapsedTicks;

                string address = message.Address.Substring(Name.Length);
                MuseData dataType = Map.AddressToData[address];

                // 만약 원하는 데이터가 아니라면 다음 패킷을 받음
                if (!DataToRead.HasFlag(dataType)) continue;

                if (dataType.CanConvertTo(typeof(Channel)))
                {
                    data[dataType].Add(new Channel(
                        (float)(double)message[0],
                        (float)(double)message[1],
                        (float)(double)message[2],
                        (float)(double)message[3],
                        packetReceivedTickCount));
                }
                else if (dataType.CanConvertTo(typeof(Direction)))
                {
                    data[dataType].Add(new Direction(
                        (float)(double)message[0],
                        (float)(double)message[1],
                        (float)(double)message[2],
                        packetReceivedTickCount));
                }
            }
        }
    }
}
