using System;
using System.Net.Sockets;
using System.Text;

namespace Lightpack
{
    public class ApiLightpack : IDisposable
    {
        private string _version;

        public string Host = "127.0.0.1";
        public int Port = 3636;

        private bool isLock = false;

        public string Version
        {
            get { return _version; }
        }

        private readonly TcpClient _client;

        public ApiLightpack()
        {
            _client = new TcpClient();
        }

        public ApiLightpack(string host, int port)
        {
            Host = host;
            Port = port;
            _client = new TcpClient();
        }

        private string ReadData()
        {
            var bytesReceived = new byte[256];
            var bytes = _client.Client.Receive(bytesReceived, bytesReceived.Length, 0);
            var data = Encoding.UTF8.GetString(bytesReceived, 0, bytes);

            return data.Replace("\n", string.Empty)
                .Replace("\r", string.Empty);
        }

        private void SendData(string data)
        {
            var bytesSent = Encoding.UTF8.GetBytes(data);
            _client.Client.Send(bytesSent);
        }

        public bool Connect()
        {
            try
            {
                _client.Connect(Host, Port);
                SetVersion(ReadData());
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void Disconnect()
        {
            _client.Close();
        }

        public Status GetStatus()
        {
            SendData("getstatus\n");
            var response = ReadData();
            var list = response.Split(':');
            if (list.Length > 1)
            {
                if (list[1] == "off") return Status.Off;
                if (list[1] == "on") return Status.On;
            }
            return Status.Error;
        }

        public void SetStatus(Status status)
        {
            if (isLock || Lock())
            {
                var s = status == Status.On ? "on" : "off";
                SendData($"setstatus:{s}\n");
                ReadData();
                UnLock();
            }
        }

        public StatusApi GetStatusApi()
        {
            SendData("getstatusapi\n");
            var response = ReadData();
            var list = response.Split(':');
            if (list.Length > 1)
            {
                var status = list[1];
                if (status == "idle") return StatusApi.Idle;
                if (status == "busy") return StatusApi.Busy;
            }
            return StatusApi.Busy;
        }

        public string GetProfile()
        {
            SendData("getprofile\n");
            var response = ReadData();
            var list = response.Split(':');
            return list.Length > 1 ? list[1] : "";
        }

        public void SetProfile(string profile)
        {
            if (isLock || Lock())
            {
                SendData($"setprofile:{profile}\n");
                ReadData();
                UnLock();
            }
        }

        public string[] GetProfiles()
        {
            SendData("getprofiles\n");
            var response = ReadData();
            var list = response.Split(':');
            return list.Length > 1 ? list[1].Split(';') : null;
        }

        private void SetVersion(string readData)
        {
            var list = readData.Split(':');
            if (list.Length > 1)
                _version = list[1];
        }

        public bool Lock()
        {
            SendData("lock\n");
            var response = ReadData();
            var list = response.Split(':');
            if (list.Length > 1)
                isLock = list[1] == "success";
            return isLock;
        }

        public void UnLock()
        {
            if (!isLock) return;
            SendData("unlock\n");
            var response = ReadData();
            var list = response.Split(':');
            if (list.Length > 1 && list[1] == "success")
                isLock = false;
        }

        public void SetAllColor(Color color)
        {
            var command = "setcolor:";
            for (int i = 0; i < 10; i++)
                command += $"{i + 1}-{color.R},{color.G},{color.B};";
            SendData($"{command}\n");
            ReadData();
        }

        public void SetColor(int num, Color color)
        {
            if (num < 1 && num > 10) return;
            var command = $"{num + 1}-{color.R},{color.G},{color.B};";
            SendData($"{command}\n");
            ReadData();
        }

        public int Smooth
        {
            set
            {
                SendData($"setsmooth:{value}\n");
                ReadData();
            }
        }

        public int Gamma
        {
            set
            {
                SendData($"setgamma:{value}\n");
                ReadData();
            }
        }

        public void Dispose()
        {
            if(_client.Connected)
                Disconnect();
        }
    }
}
