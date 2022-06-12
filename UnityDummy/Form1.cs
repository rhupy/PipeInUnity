using System.IO.Pipes;
using System.Security.Principal;
using System.Text;

namespace UnityDummy
{
    public partial class Form1 : Form
    {
        #region Pipe통신
        //파이프
        NamedPipeClientStream PipeClient = new NamedPipeClientStream(".", "UI to Unity", PipeDirection.Out, PipeOptions.None, TokenImpersonationLevel.Impersonation);
        NamedPipeServerStream PipeServer = new NamedPipeServerStream("Unity to UI", PipeDirection.In);
        String ReturnString
        {
            set
            {
                this.Invoke(new EventHandler(delegate {
                    richTextBox1.AppendText(value + "\r\n");
                    richTextBox1.SelectionStart = richTextBox1.Text.Length;
                    richTextBox1.ScrollToCaret();
                }));
            }
        }
        String InputString
        {
            get
            {
                string r = string.Empty;
                this.Invoke(new EventHandler(
                        delegate
                        {
                            r = richTextBox2.Text;
                        }));
                return r;
            }
        }
        public Form1()
        {
            InitializeComponent();

            //메세지 받는 파이프 준비
            Task.Factory.StartNew(() => PipeRecieve());
        }
        public void PipeRecieve()
        {
            PipeServer.WaitForConnection();
            ReturnString = "PipeServer 연결 생성";
            while (PipeServer.IsConnected)
            {
                var read = new byte[4096];
                int iRet = PipeServer.Read(read, 0, read.Length);
                if (iRet > 0)
                {
                    var msg = Encoding.UTF8.GetString(read);
                    ReturnString = "[Recieve]" + msg;
                }
            }
        }
        public void PipeSend()
        {
            string msg = InputString;
            while (true)
            {
                if (!PipeClient.IsConnected)
                {
                    try
                    {
                        PipeClient.Connect();
                        Task.Delay(500);
                        ReturnString = "PipeClient 연결 생성";
                    }
                    catch (Exception ee)
                    {
                        ReturnString = ee.Message;
                    }
                    continue;
                }

                try
                {
                    StreamWriter writer = new StreamWriter(PipeClient);
                    writer.WriteLine(msg);
                   
                    this.Invoke(new EventHandler(
                        delegate
                        {
                            ReturnString = "[Send]" + msg;
                        }));
                    writer.Flush();
                }
                catch (Exception ee)
                {
                    ReturnString = ee.Message;
                }
                break;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Task.Factory.StartNew(() => PipeSend());
        }
        #endregion
    }
}