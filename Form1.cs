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
            xboxConsole.CallVoid(0x8242FB70, new object[] { -1, -1,  "v cg_fov 120" });
        }

        
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                if (checkBox1.Checked) // BO 2
                {

                    uint[] addresses = { 0xC3781E3C,0xC3781E44,0xC3781E40 };
                    for (int i = 0; i < addresses.Length; i++)
                    {
                        console.SetMemory(addresses[i], new byte[] { 0x27, 0x0F });
                    }
                }
                else if (checkBox2.Checked)
                {
                    console.SetMemory(0xC35126DC, new byte[] { 0x64 });
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

        private void button3_Click_1(object sender, EventArgs e)
        {
            
            
        }

        private void tabPage2_Click(object sender, EventArgs e)
        {

        }
    }
}
