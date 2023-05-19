﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ServerSide
{
    public partial class Form1 : Form
    {

        List<string> users = new List<string>();
        List<Button> players = new List<Button>();
        List<bool> statusCrashed = new List<bool>();
        List<Point> pos = new List<Point>();

        List<Point> enemyPos = new List<Point>();
        List<Button> EnemyCar = new List<Button>();
        List<int> EnemySpeed = new List<int>();
        public int globalport = 11000;
        int numCars = 5;

        public int[,] randpos = new int[,] {
            { 10,  10 },
            { 100, 130},
            { 230, 280},
            { 370, 390},
            { 490, 500},
        };
        public int[] randspd = new int[] { 20, 50, 10, 40, 20, 35, 30, 15, 45, 25, 20 };
        public Random rndm = new Random();

        public Form1()
        {
            InitializeComponent();
            pictureBox2.SendToBack();
        }

        private void button1_Click(object sender, EventArgs e)      //Running game on click and removing start button
        {
            this.Controls.Remove(Start);

            label11.Text = "Dodge EM Cars";

            run();
        }

        public void run()
        {
            Thread s = new Thread(move);
            s.Start();

            for (int i = 0; i < numCars; i++)
            {
                int id = i;
                Thread sC = new Thread(() => spawnCars(id));
                sC.Start();
            }
        }


        private void Form1_Load_1(object sender, EventArgs e)
        {
            string[] ips = "112,116,117,113,128,126,115".Split(',');

            for (int i = 0; i < ips.Length; i++)
            {
                spawn(Convert.ToInt32(ips[i]), i);
            }
        }
        
        public void spawn(int i, int id)
        {
            users.Add("192.168.0." + i);
            Button b = new Button();
            b.Text = i.ToString();
            b.Width = 60;
            b.Height = 70;
            b.BackColor = Color.Green;
            b.ForeColor = Color.White;
            this.Controls.Add(b);
            players.Add(b);
            pos.Add(new Point(250, 280));
            statusCrashed.Add(false);

            Thread n = new Thread(() => read(id, i));
            n.Start();
        }

        public void read(int id, int ipp)
        {
            statusCrashed[id] = false;
            int listenPort = globalport;
            using (UdpClient listener = new UdpClient(ipp * 10))
            {

                while (!statusCrashed[id])
                {
                    IPEndPoint listenEndPoint = new IPEndPoint(IPAddress.Parse(users[id]), ipp * 10);
                    byte[] receivedData = listener.Receive(ref listenEndPoint);
                    string n = Encoding.Unicode.GetString(receivedData);

                    pos[id] = new Point(Convert.ToInt32(n.Split(',')[0]), Convert.ToInt32(n.Split(',')[1]));
                    //should be "Hello World" sent from above client
                }


            }
        }

        public void send(int id)
        {
            // write send code here for updating screen on user screens with broadcast

        }


        public void set()
        {
            if (this.InvokeRequired == true)
            {
                this.Invoke(new MethodInvoker(set));
            }
            else
            {
                for (int i = 0; i < players.Count; i++)
                {
                    players[i].Left = pos[i].X;
                    players[i].Top = pos[i].Y;
                }
            }
        }
        public void move()
        {
            while (true)
            {
                try
                {
                    set();
                    Thread.Sleep(1);
                }
                catch (Exception e)
                {

                }
            }

        }


        // Enemy Cars Spawning ---------------=================================


        public void spawnCars(int i)
        {
            Button b = new Button();
            b.Text = "Car";
            b.Width = 60;
            b.Height = 70;
            b.BackColor = Color.Yellow;
            b.ForeColor = Color.Black;
            EnemyCar.Add(b);

            int num = rndm.Next(0, 10);
            EnemySpeed.Add(randspd[num]);

            num = rndm.Next(0, 2);
            enemyPos.Add(new Point(randpos[i, num], -20));

            spawnCarUpdate(b);

            Thread m = new Thread(() => moveenemy(i));
            m.Start();

            Thread c = new Thread(() => collision(i));
            c.Start();
        }

        public void spawnCarUpdate(Button b)
        {
            if (this.InvokeRequired == true)
            {
                this.Invoke(new MethodInvoker(() => spawnCarUpdate(b)));
            }
            else
            {
                this.Controls.Add(b);
            }
        }

        public void setenemy(int i)                 //Enemy positions
        {
            if (this.InvokeRequired == true)
            {
                this.Invoke(new MethodInvoker(()=> setenemy(i)));
            }
            else
            {
                enemyPos[i] = new Point(enemyPos[i].X, enemyPos[i].Y + EnemySpeed[i]);
                EnemyCar[i].Top += EnemySpeed[i];
                EnemyCar[i].Left = enemyPos[i].X;
                if (enemyPos[i].Y > 400)
                {
                    int num = rndm.Next(0, 2);
                    enemyPos[i] = new Point(randpos[i, num], -20);
                    num = rndm.Next(0, 5);
                    EnemySpeed[i] = randspd[num];
                    EnemyCar[i].Top = enemyPos[i].Y;
                    EnemyCar[i].Left = enemyPos[i].X;
                }
            }
        }

        public void moveenemy(int i)    
        {
            while (true)
            {
                setenemy(i);
                Thread.Sleep(300);
            }
        }

        public void collision(int i)        //Enemy and Player collision
        {
            while (true)
            {
                for (int j = 0; j < players.Count; j++)
                {
                    double res = Math.Sqrt(Math.Pow(pos[j].X - enemyPos[i].X, 2) + Math.Pow(pos[j].Y - enemyPos[i].Y, 2));
                    if (res < 60)
                    {
                        pos[j] = new Point(5000, 5000);
                        statusCrashed[j] = true;
                        crashMessage(j);
                    }
                }
            }
        }
        public void crashMessage(int j)
        {
            if (this.InvokeRequired == true)
            {
                this.Invoke(new MethodInvoker(() => crashMessage(j)));
            }
            else
            {
                label12.Text = users[j] + "\n Has Crashed !!\n\n" + label12.Text ;
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }
    }
}