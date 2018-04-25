using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Linq;




public class ClientTchpConnect : MonoBehaviour
{
    //Heavy/long message but i was thinking for a project that don't communicate every second, with a low number of player.
    private List<Byte[]> ListOfMessage;
    const string ALIVE_MESSAGE = "STILL ALIVE\n";
    const string QUIT_CLIENT_MESSAGE = "I Quit Have A Good Day !\n";
    const string ERROR_MESSAGE = "ERROR MESSAGE, SEND AGAIN";


    const string REPLY_MESSAGE = "ROGER\n";
    const string QUIT_SERVER_MESSAGE = "Server Close, THANKS YOU GUYS\n";
    const int DEFAULT_PORT = 8052;
    #region private members
    private Thread clientReceiveThread;
    #endregion
    private String data = null;
    public int Bignbr = 0;
    public bool ToQuit = false;
    //timer to stayt alive
    Timer StayAlivetimer;

    private TcpClient socketConnection;

    // Use this for initialization 	
    void Start()
    {
        Debug.Log("Bonjour je suis client");
        ConnectToTcpServer();
    }
    // Update is called once per frame
    void Update()
    {
    }

    /// <summary> 	
    /// Setup socket connection. 	
    /// </summary> 	
    private void ConnectToTcpServer()
    {
        try
        {
            clientReceiveThread = new Thread(new ThreadStart(ListenForData));
            clientReceiveThread.IsBackground = true;
            clientReceiveThread.Start();
            StayAlivetimer = new Timer(stay_alive, null, 0 * 1000, Timeout.Infinite);
        }
        catch (Exception e)
        {
            Debug.Log("On client connect exception " + e);
        }
    }
    /// <summary> 	
    /// Runs in background clientReceiveThread; Listens for incomming data. 	
    /// </summary>     
    private void ListenForData()
    {
        try
        {
            //ATTENTION : mettre l'adresse ip local ou celle du serveur dédié, faite attention au autre logiciel comme skype quui sont succeptible doccuper certains ports sur certaines connections
            socketConnection = new TcpClient("10.91.29.16", DEFAULT_PORT);
            Byte[] bytes = new Byte[1024];
            while (ToQuit != true)
            {
                // Get a stream object for reading 				
                using (NetworkStream stream = socketConnection.GetStream())
                {
                    int length;
                    // Read incomming stream into byte arrary. 					
                    while ((length = stream.Read(bytes, 0, bytes.Length)) >= 0)
                    {
                        var incommingData = new byte[length];
                        Array.Copy(bytes, 0, incommingData, 0, length);
                        // Convert byte array to string message. 						
                        string serverMessage = Encoding.ASCII.GetString(incommingData);
                        Debug.Log("client received message : " + serverMessage);
                    }
                }
            }
            CloseThread();
            return;
        }
        catch (SocketException socketException)
        {
            Debug.Log("Socket exception: " + socketException);
        }
    }
    /// <summary> 	
    /// Send message to server using socket connection. 	
    /// </summary> 	
    public void SendMessage(string message)
    {
        if (socketConnection == null)
            return;
        try
        {
            // Get a stream object for writing. 			
            NetworkStream stream = socketConnection.GetStream();
           
            if (stream.CanWrite)
            {
                // Write byte array to socketConnection stream.
                byte[] datamessage = Encoding.ASCII.GetBytes(message);
                stream.Write(datamessage, 0, datamessage.Length);
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("Socket exception: " + socketException);
        }
    }

    private void stay_alive(object ob)
    {
        SendMessage(ALIVE_MESSAGE);
        StayAlivetimer.Change(10 * 1000, Timeout.Infinite);
    }

    private void OnApplicationQuit()
    {
        Debug.Log("clienht app");
        ToQuit = true;
        socketConnection.Close();
        clientReceiveThread.Abort();
        StayAlivetimer.Dispose();
    }

    public void CloseThread()
    {
        Debug.Log("client abort");
        socketConnection.Close();
        StayAlivetimer.Dispose();
        clientReceiveThread.Abort();
    }
}
