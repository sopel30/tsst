﻿using networkLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace ClientNode
{
    class Client
    {
        private transportClient client;
        private Config conf;
        private Grid chat;
        private TextBlock status;
        private int messageNumber = 0;
        private int rIndex;
        private string nodeName { get; set; }
        private string CloudIP { get; set; }
        private string CloudPort { get; set; }
        private List<string> portsIn { get; set; }
        private List<string> portsOut { get; set; }

        public Client(Grid chat, TextBlock status)
        {
            this.chat = chat;
            this.status = status;
            rIndex = Grid.GetRow(chat);
        }

        public void startService()
        {
            try
            {
                client = new transportClient(CloudIP, CloudPort);
                client.OnNewMessageRecived += new transportClient.NewMsgHandler(newMessageRecived);
                displayStatusMessage(Constants.SERVICE_START_OK, Constants.LOG_INFO);
            }

            catch
            {
                displayStatusMessage(Constants.SERVICE_START_ERROR, Constants.LOG_ERROR);

            }
        }

        public bool isStarted()
        {
            if (client != null && client.isConnected())
            {
                return true;
            }

            else
            {
                return false;
            }
        }

        public void readConfig(string path)
        {
            try
            {
                conf = new Config(path, Constants.Client);
                this.nodeName = conf.config[0];
                this.CloudIP = conf.config[1];
                this.CloudPort = conf.config[2];
                displayStatusMessage(Constants.CONFIG_OK, Constants.LOG_INFO);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.StackTrace);
                displayStatusMessage(Constants.CONFIG_ERROR + "...", Constants.LOG_ERROR);
            }
        }
        private void newMessageRecived(object a, MessageArgs e)
        {
            addChatMessage(e.Message, Constants.RIGHT);
        }

        public void sendMessage(string msg)
        {
            client.sendMessage(msg);
            addChatMessage(msg, Constants.LEFT);
        }

        private void displayStatusMessage(string message, int type)
        {
            var color = Brushes.Black;

            switch (type)
            {
                case networkLibrary.Constants.LOG_ERROR:
                    color = Brushes.Red;
                    break;
                case networkLibrary.Constants.LOG_INFO:
                    color = Brushes.Black;
                    break;
            }

            status.Text = message;
            status.Foreground = color;

        }

        private void addChatMessage(string msg, int aligment)
        {
            var algmnt = System.Windows.TextAlignment.Left;
            switch (aligment)
            {   
                case Constants.LEFT:
                    algmnt = System.Windows.TextAlignment.Left;
                    break;
                case Constants.RIGHT:
                    algmnt = System.Windows.TextAlignment.Right;
                    break;
            }

            this.chat.Dispatcher.Invoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                new Action(() =>
                {
                    var t = new TextBlock();
                    t.Text = ("[" + DateTime.Now.ToString("HH:mm:ss") + "]" + Environment.NewLine +
                        msg + Environment.NewLine + Environment.NewLine);
                    t.TextAlignment = algmnt;
                    RowDefinition gridRow = new RowDefinition();
                    gridRow.Height = new GridLength(35);
                    chat.RowDefinitions.Add(gridRow);
                    Grid.SetRow(t, messageNumber);
                    messageNumber++;
                    chat.Children.Add(t);
                })
            );
        }

    }
}
