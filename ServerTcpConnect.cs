using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Linq;




public class ServerTcpConnect : MonoBehaviour
{
    //Heavy/long message but it was thinking for a project that don't communicate every second, with a low number of player.
    private List<Byte[]> ListOfMessage;
    const string ALIVE_MESSAGE = "STILL ALIVE\n";
    const string QUIT_CLIENT_MESSAGE = "I Quit Have A Good Day !\n";
    const string ERROR_MESSAGE = "ERROR MESSAGE, SEND AGAIN";


    const string REPLY_MESSAGE = "ROGER\n";
    const string QUIT_SERVER_MESSAGE = "Server Close, THANKS YOU GUYS\n";
    const int DEFAULT_PORT = 8052;
    #region private members 	
    /// <summary> 	
    /// TCPListener to listen for incomming TCP connection 	
    /// requests. 	
    /// </summary> 	
    private TcpListener tcpListener;
    /// <summary> 
    /// Background thread for TcpServer workload. 	
    /// </summary> 	
    private List<Thread> tcpListenerThread;
    public bool EndThread = false;
    /// <summary> 	
    /// Create handle to connected tcp client. 	
    /// </summary>
    private List<int> IdClient;
    public List<TcpClient> TcpClients;
    #endregion
    public int nbr = 0;

    public List<string> ReceivedMessage;
    String MessageToAll = null;
    public bool SentSentToAllBool = false;

    //delegate pointer to function for basic message received
    delegate void DelegateMessage(object ob);
    private List<DelegateMessage> DM;
    // time for client alive/dead
    private List<Timer> ClientTimer;



    // Use this for initialization
    void Start()
    {
        Debug.Log("Bonjour je suis serveur");
        //convert string message to byte array for faster check up when receving message
        ListOfMessage = new List<byte[]>();
        ListOfMessage.Add(Encoding.ASCII.GetBytes(ERROR_MESSAGE));
        // last two message need to be athe end of list
        ListOfMessage.Add(Encoding.ASCII.GetBytes(ALIVE_MESSAGE));
        ListOfMessage.Add(Encoding.ASCII.GetBytes(QUIT_CLIENT_MESSAGE));

        //launch server
        tcpListener = new TcpListener(IPAddress.Parse("10.91.29.16"), DEFAULT_PORT);
        tcpListener.Start();

        TcpClients = new List<TcpClient>();
        IdClient = new List<int>();

        // Start TcpServer background thread
        tcpListenerThread = new List<Thread>();
        /*tcpListenerThread.Add(new Thread(new ThreadStart(ListenForIncommingRequests)));
        tcpListenerThread[0].IsBackground = true;
        tcpListenerThread[0].Start();*/

        DM = new List<DelegateMessage>();
        DM.Add(ErrorMessage);
        DM.Add(ResetTimerClient);
        DM.Add(clientQuit);

        ClientTimer = new List<Timer>();

    }

    // Update is called once per frame
    void Update()
    {
        if (tcpListener.Pending() == true)
        {
            TcpClient client = tcpListener.AcceptTcpClient();
            AddNewClient(client);
        }
        if (SentSentToAllBool)
        {
            lock (MessageToAll)
            {
                foreach (TcpClient client in TcpClients)
                    SendMessage(MessageToAll, client);
                SentSentToAllBool = false;

            }
        }
    }

    /// <summary> 	
    /// Runs in background TcpServerThread; Handles incomming TcpClient requests 	
    /// </summary> 	
    public void ListenForIncommingRequests()
    {
        int nbr = 0;
        int index = 0;
        for (index = 0; index < IdClient.Count && IdClient[index] != 0; ++index) ;
        
        if (index >= IdClient.Count)
        {
            CloseThread();
            return;
        }
        lock (IdClient){
            IdClient[index] = 1;
        }

        TcpClient ClientToListen = TcpClients[index];
        try
        {
            Byte[] bytes = new Byte[1024];
            while (EndThread == false)
            {
                // Get a stream object for reading
                using (NetworkStream stream = ClientToListen.GetStream())
                {
                    int length;
                    // Read incomming stream into byte arrary.
                    while ((length = stream.Read(bytes, 0, bytes.Length)) >= 0)
                    {
                        var incommingData = new byte[length];
                        Array.Copy(bytes, 0, incommingData, 0, length);
                        for (nbr = 0; nbr < ListOfMessage.Count && !ListOfMessage[nbr].SequenceEqual(incommingData); ++nbr);
                        if (nbr >= ListOfMessage.Count)
                        {
                            String pre_check = Encoding.ASCII.GetString(incommingData);
                            int[] check_error = CountStringOccurence(pre_check, ':', ';');
                            if (check_error[0] != (check_error[1] / 3))
                                {
                                    ErrorMessage(index);
                                    return;
                                }
                            Debug.Log("received message :" + pre_check);
                        }
                        else
                            DM[nbr](index);
                        Debug.Log("still in");
                    }
                }
            }
            clientQuit(IdClient);
            return;
        }
        catch (SocketException socketException)
        {
            Debug.Log("SocketException " + socketException.ToString());
        }
    }

    private int[] CountStringOccurence(string str, char first, char second)
    {
        int nbr = 0, i = 0, x = 0, end = str.Length;
        while (nbr < end)
        {
            if (str[nbr] == first)
                ++i;
            else if (str[nbr] == second)
                ++x;
            ++nbr;
        }
        return (new int[] { i, x });
    }

    /// <summary> 	
    /// Send message to client using socket connection. 	
    /// </summary> 	
    public void SendMessage(string Message, TcpClient ClientToSend)
    {
        if (ClientToSend == null)
            return;
        try
        {
            // Get a stream object for writing. 			
            NetworkStream stream = ClientToSend.GetStream();
            if (stream.CanWrite)
            {
                // Convert string message to byte array.                 
                byte[] serverMessageAsByteArray = Encoding.ASCII.GetBytes(Message);
                // Write byte array to socketConnection stream.               
                stream.Write(serverMessageAsByteArray, 0, serverMessageAsByteArray.Length);
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("Socket exception: " + socketException);
        }
    }

    // New Client, received port and listen
    public void AddNewClient(TcpClient client)
    {
        Debug.Log("addd cliebnt");
        lock (IdClient) {
            lock (TcpClients) {
                lock (tcpListenerThread) {
                    IdClient.Add(0);
                    TcpClients.Add(client);
                    tcpListenerThread.Add(new Thread(new ThreadStart(ListenForIncommingRequests)));
                    tcpListenerThread[(tcpListenerThread.Count - 1)].IsBackground = true;
                    tcpListenerThread[(tcpListenerThread.Count - 1)].Start();
                    ClientTimer.Add(new Timer(clientQuit, (TcpClients.Count - 1), 20 * 1000, Timeout.Infinite));
                }
            }
        }
    }


    //Stille alive, reset timer for client
    public void ResetTimerClient(object ob)
    {
        int id = Int32.Parse(ob.ToString());
        if (ClientTimer.Count <= 0 || id > ClientTimer.Count)
            return;
        lock (ClientTimer)
        {
            ClientTimer[id].Change(20 * 1000, Timeout.Infinite);
        }
    }

    // on application quit
    public void OnApplicationQuit()
    {
        Debug.Log("server app");
        QuitAllClient();
    } 

    // Quti all client 
    public void QuitAllClient()
    {

        EndThread = true;
        lock (tcpListenerThread)
        {
            lock (TcpClients)
            {
                lock (ClientTimer)
                {
                    foreach (TcpClient cl in TcpClients)
                        cl.Close();
                    foreach (Thread th in tcpListenerThread)
                        th.Abort();
                    foreach (Timer TI in ClientTimer)
                        TI.Dispose(); ;
                }
            }
        }
        return;
    }

    // quit client, delete him
    public void clientQuit(object ob)
    {
        Debug.Log("server clienrt abort");
        int id = Int32.Parse(ob.ToString());
        lock (ClientTimer)
        {
            lock (tcpListenerThread)
            {
                lock (TcpClients)
                {
                    lock (IdClient)
                    {
                        ClientTimer[id].Dispose();
                        ClientTimer.RemoveAt(id);
                        tcpListenerThread.RemoveAt(id);
                        IdClient.RemoveAt(id);
                        TcpClients[id].Close();
                        TcpClients.RemoveAt(id);
                        
                        CloseThread();
                        return;
                    }
                }
            }
        }
    }

    //Error message send message again
    private void ErrorMessage(object ob)
    {

    }

    private void CloseThread()
    {
        Thread.ResetAbort();
    }

}
