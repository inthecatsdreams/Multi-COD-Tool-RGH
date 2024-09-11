﻿using System;
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
        string cpuTemp, gpuTemp, edramTemp, moboTemp;


        public bool connected = false;
        
        
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (connected == false)
                button5.Enabled = false;


            label1.Text = "IP: Waiting";
            label2.Text = "CPU: waiting";
            label3.Text = "GPU: waiting";
            label4.Text = "EDRAM: waiting";
            label5.Text = "MOBO: waiting";
            label1.ForeColor = Color.Red;
            label2.ForeColor = Color.Red;
            label3.ForeColor = Color.Red;
            label4.ForeColor = Color.Red;
            label5.ForeColor = Color.Red;
            label6.ForeColor = Color.Red;   
        }

        private void button1_Click(object sender, EventArgs e)
        {
            console.Connect();
            xboxConsole.Connect(out xboxConsole);
            label1.Text = "Status: connected to ";
            string consoleIP = xboxConsole.XboxIP();
            label1.Text += consoleIP;
            label1.ForeColor = Color.Green;

            if (backgroundWorker1.IsBusy == false)
                backgroundWorker1.RunWorkerAsync();
            if (console.activeConnection == true)
            {
                
                MessageBox.Show("Connected!");
                connected = true;
                button5.Enabled = true;
                cpuTemp = xboxConsole.GetTemperature(JRPC.TemperatureType.CPU).ToString();
                gpuTemp = xboxConsole.GetTemperature(JRPC.TemperatureType.GPU).ToString();
                edramTemp = xboxConsole.GetTemperature(JRPC.TemperatureType.EDRAM).ToString();
                moboTemp = xboxConsole.GetTemperature(JRPC.TemperatureType.MotherBoard).ToString();
                label2.Text = "CPU: " +cpuTemp + "°C";
                label3.Text = "GPU: "+ gpuTemp + "°C";
                label4.Text = "EDRAM: " +edramTemp + "°C";
                label5.Text = "MOBO: " +moboTemp + "°C";
                label2.ForeColor = Color.Green;
                label3.ForeColor = Color.Green;
                label4.ForeColor = Color.Green; 
                label5.ForeColor = Color.Green;
                
                string[] subs = xboxConsole.RunningProcessInfo.ProgramName.ToString().Split('\\');
                label6.Text = subs[5] + " - "+ subs[6];
                
                label6.ForeColor = Color.Green;

            }

            
        }

        

        private void button4_Click(object sender, EventArgs e)
        {
            console.SetMemory(0x82259BC8, new byte[] { 0x60, 0x00, 0x00, 0x00 });
        }

        

        private void button6_Click(object sender, EventArgs e)
        {
            console.SetMemory(0xC3786FD8, new byte[] { 0x00, 0x0F, 0x42, 0x40});
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

       

        

        private void button9_Click(object sender, EventArgs e)
        {
            console.SetMemory(0x821154A4, new byte[] { 0x60, 0x00, 0x00, 0x00 });
        }

        private void button5_Click(object sender, EventArgs e)
        {
            cpuTemp = xboxConsole.GetTemperature(JRPC.TemperatureType.CPU).ToString();
            gpuTemp = xboxConsole.GetTemperature(JRPC.TemperatureType.GPU).ToString();
            edramTemp = xboxConsole.GetTemperature(JRPC.TemperatureType.EDRAM).ToString();
            moboTemp = xboxConsole.GetTemperature(JRPC.TemperatureType.MotherBoard).ToString();
            label2.Text = "CPU: " + cpuTemp + "°C";
            label3.Text = "GPU: " + gpuTemp + "°C";
            label4.Text = "EDRAM: " + edramTemp + "°C";
            label5.Text = "MOBO: " + moboTemp + "°C";
            label2.ForeColor = Color.Green;
            label3.ForeColor = Color.Green;
            label4.ForeColor = Color.Green;
            label5.ForeColor = Color.Green;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void button10_Click(object sender, EventArgs e)
        {
            console.SetMemory(0x821614D4, new byte[] { 0x60, 0x00, 0x00, 0x00 });
        }

        private void button11_Click(object sender, EventArgs e)
        {
            console.SetMemory(0x8210E58C, new byte[] { 0x3B, 0x80, 0x00, 0x01 });
        }
        

        private void button12_Click(object sender, EventArgs e)
        {
            
            
        }

        
    }
}
