using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using XboxCore.Xbox.Connection;
using XDevkit;
using XRPCLib;

namespace JTAGTool
{
    public partial class Form1 : Form
    {


        XRPC console = new XRPC();
        IXboxConsole xboxConsole;
        

        
        public void iPrintInBold(string text)
        {
            xboxConsole.CallVoid(0x8242FB70, new object[] { -1, 0, "; " + text });
        }
        
        
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            label1.Text = "Status: Waiting";
            label1.ForeColor = Color.Red;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            console.Connect();
            xboxConsole.Connect(out xboxConsole);
            label1.Text = "Status: connected to ";
            string consoleState = xboxConsole.XboxIP();
            label1.Text += consoleState;
            label1.ForeColor = Color.Green;
            if (backgroundWorker1.IsBusy == false)
                backgroundWorker1.RunWorkerAsync();
            if (console.activeConnection == true)
            {
                MessageBox.Show("Connected!");
            }
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            console.SetMemory(0x821F5B7F, new byte[] { 0x01 });

        }

        private void button4_Click(object sender, EventArgs e)
        {
            console.SetMemory(0x82259BC8, new byte[] { 0x60, 0x00, 0x00, 0x00 });
        }

        private void button5_Click(object sender, EventArgs e)
        {
            string textToSend = textBox1.Text;

            if (textToSend != "" || textToSend.Length != 0) { 
                iPrintInBold(textToSend);
            }

            else
            {
                MessageBox.Show("Text Field can't be empty");
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            console.SetMemory(0xC3786FD8, new byte[] { 0x00, 0x0F, 0x42, 0x40});
        }

        private void button7_Click(object sender, EventArgs e)
        {
            xboxConsole.CallVoid(0x8242FB70, new object[] { -1, -1, "q cg_fov 0-9999" });
        }

        
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                if (checkBox1.Checked) // BO II
                {

                    uint[] addresses = { 0xC3781E3C,0xC3781E44,0xC3781E40 };
                    for (int i = 0; i < addresses.Length; i++)
                    {
                        console.SetMemory(addresses[i], new byte[] { 0x27, 0x0F });
                    }
                }
                else if (checkBox2.Checked) // BO I
                {
                    console.SetMemory(0xC35126DC, new byte[] { 0x64 });
                }
                else if (checkBox3.Checked) // BO I
                {
                    uint[] addresses = { 0xC3381BE8, 0xC3381BF8, 0xC3381C00 };
                    for(int i =  0; i < addresses.Length; i++)
                    {
                        console.SetMemory(addresses[i], new byte[] { 0x27, 0x0F });
                    }
                }
                else if (checkBox4.Checked)
                {
                    uint[] adresses = { 0xC458B28C, 0xC458B270 };
                    for(int i =0; i < adresses.Length; i++)
                    {
                        console.SetMemory(adresses[i], new byte[] { 0x00,0xFF,0xFF, 0xFF });
                    }
                }
                else if (checkBox5.Checked)
                {
                    console.SetMemory(0xC470A44C, new byte[] { 0x00, 0xFF,0xFF, 0xFF  });
                }
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            console.SetMemory(0xC33833B4, new byte[] { 0x00, 0x0F, 0x42, 0x40 });
        }

        
        private void tabPage2_Click(object sender, EventArgs e)
        {

        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            console.SetMemory(0xC458CCBC, new byte[] { 0x00, 0x0F, 0x42, 0x40 });
        }

        private void button7_Click_1(object sender, EventArgs e)
        {
            uint prestige = 0x84085720 + 0x90DD;
            int prestigeInt = 15;
            uint rank = 0x84085720 + 0x90E1;
            int rankInt = 50;
            uint rankXP = 0x84085720 + 0x90E5;
            uint codPoints = 0x84085720 + 0x8CD1;
            console.SetMemory(prestige, BitConverter.GetBytes(prestigeInt));
            console.SetMemory(rank, BitConverter.GetBytes(rankInt));
            console.SetMemory(rankXP, BitConverter.GetBytes(9999999));
            console.SetMemory(codPoints, BitConverter.GetBytes(999999));

                //BitConverter.GetBytes(desiredValue);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            uint prestige = 0x8307B76C + 0x3A90C;
            uint prestigeToken = 0x8307B76C + 0x3C76B;
            int prestigeInt = 10;
            int tokenInt = 80;
            uint rankXP = 0x8307B76C + 0x3AB2C;
            uint token = 0x8307B76C + 0x3C7C9;
            
            
            //console.SetMemory(prestige, BitConverter.GetBytes(prestigeInt));
            //console.SetMemory(token, BitConverter.GetBytes(tokenInt));
            //console.SetMemory(rankXP, BitConverter.GetBytes(9999999));
            
        }

        private void button9_Click(object sender, EventArgs e)
        {
            console.SetMemory(0x821154A4, new byte[] { 0x60, 0x00, 0x00, 0x00 });
        }

        private void button10_Click(object sender, EventArgs e)
        {
            console.SetMemory(0x821614D4, new byte[] { 0x60, 0x00, 0x00, 0x00 });
        }

        private void button11_Click(object sender, EventArgs e)
        {
            console.SetMemory(0x8210E58C, new byte[] { 0x3B, 0x80, 0x00, 0x01 });
        }
        uint ReverseBytes(uint val) //gotta convert all stat values to small endian
        {
            byte[] toBytes = BitConverter.GetBytes(val);
            Array.Reverse(toBytes);
            return BitConverter.ToUInt32(toBytes, 0);
        }
        public uint[] accolades = new uint[47];
        void SetAccoladesValues(uint value)
        {
            
            for (int i = 0; i < 47; i++)
                accolades[i] = ReverseBytes(value);
        }

        private void button12_Click(object sender, EventArgs e)
        {
            
            SetAccoladesValues(9999);
            for (uint i = 0; i > 47; i++)
                xboxConsole.WriteUInt32(0x830A60D0 + i, accolades[i]);
        }

        private void button13_Click(object sender, EventArgs e)
        {
            uint prestige = 0x843491A4;
            int prestigeInt = 11;
            uint rankXP = 0x843491BC;
            console.SetMemory(prestige, BitConverter.GetBytes(prestigeInt));
            console.SetMemory(rankXP, BitConverter.GetBytes(9999999));
            
        }
    }
}
