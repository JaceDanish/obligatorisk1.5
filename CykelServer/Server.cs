using ClassLibraryCykel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CykelServer
{
    public class Server
    {
        static List<Cykel> cykelList = new List<Cykel>()
        {
            new Cykel(1, "Sort", 1999.95, 8),
            new Cykel(2, "Hvid", 2999.95, 6),
            new Cykel(3, "Graa", 2500, 12),
            new Cykel(4, "Hvid", 1999, 16),
            new Cykel(5, "Gul", 2900, 7),
            new Cykel(6, "Groen", 4999.99, 21),
            new Cykel(7, "Blaa", 4000, 18),
            new Cykel(8, "Lilla", 1799.99, 5),
            new Cykel(9, "Pink", 4500, 5),
            new Cykel(10, "Sort", 1500, 3),
            new Cykel(11, "Blaa", 1999.95, 6)
        };

        public void Run()
        {
            TcpListener listener = new TcpListener(IPAddress.Loopback ,4646);
            listener.Start();

            while (true)
            {
                TcpClient socket = listener.AcceptTcpClient();
                Task.Run(() =>
                {
                    TcpClient tempSocket = socket;
                    DoClient(tempSocket);
                });
            }

        }

        private void DoClient(TcpClient socket)
        {
            NetworkStream ns = socket.GetStream();
            StreamReader sr = new StreamReader(ns);
            StreamWriter sw = new StreamWriter(ns);
            sw.AutoFlush = true;

            while (socket.Connected)
            {
                try
                {
                    Task task = Task.Run(async () => await RequestIO(sr, sw));
                    task.Wait();
                }
                catch (Exception e)
                {
                    socket.Close();
                }
            }

        }
        private async Task RequestIO(StreamReader sr, StreamWriter sw)
        {
            do
            {
                string inpStr1 = null;
                string inpStr2 = null;

                while (inpStr1 == null || inpStr2 == null)
                {
                    inpStr1 = sr.ReadLine();
                    inpStr2 = sr.ReadLine();
                }

                if (inpStr1 != null && inpStr1.Equals("HentAlle"))
                {
                    sw.WriteLine(GetAll());
                    break;
                }

                if (inpStr1 != null && inpStr1.Equals("Hent"))
                {
                    String inpJson = null;
                    int id = int.Parse(inpStr2);
                    if (id > 0)
                    {
                        inpJson = GetFromId(id);
                        if (inpJson != null)
                        {
                            sw.WriteLine(inpJson);
                            break;
                        }
                        else
                        {
                            sw.WriteLine("No match for ID");
                            continue;
                        }
                    }
                }

                if (inpStr1 != null && inpStr1.Equals("Gem"))
                {
                    SaveCykel(inpStr2);
                    break;
                }

            } while (true);
            
        }

        private String GetFromId(int id)
        {
            foreach (Cykel c in cykelList)
            {
                if (c.Id == id)
                {
                    return JsonConvert.SerializeObject(c);
                }
            }

            return null;
        }

        private String GetAll()
        {
            return JsonConvert.SerializeObject(cykelList);
        }

        private void SaveCykel(String inpJson)
        {
            try
            {
                Cykel cykel = JsonConvert.DeserializeObject<Cykel>(inpJson);
                if (cykel is Cykel)
                {
                    cykelList.Add(cykel);
                }
            }
            catch(Exception e)
            {
                //Keeps connection when input string is bad
            }
        }
    }
}
