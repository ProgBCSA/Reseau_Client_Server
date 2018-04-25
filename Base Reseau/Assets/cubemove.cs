using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using UnityEngine.UI;
using System.Net;
using System.Net.Sockets;

public class cubemove : MonoBehaviour {

   
    ServerTcpConnect server = null;
    List<ClientTchpConnect> clients = null;

    int status = 0;
    bool IsClientAndServerInit = false;
    bool test = true;
    public Text InputString;
    public string inputfield;
    private bool doOnce = false;


    private void Start()
    {
        clients = new List<ClientTchpConnect>();
    }

    
    public void GetIP ()
    {
        inputfield = InputString.text;
        print(inputfield);
    }

    void OnGUI()
    {
        if (Input.GetKeyDown(KeyCode.P) && status == 0)
        {
            status = 1;
            IsClientAndServerInit = false;
            if (server != null && server.TcpClients.Count > 0)
            {
                foreach (TcpClient cl in server.TcpClients)
                    server.SendMessage("HEY IM SERVER B***H !", cl);
            }
        }
        else if (Input.GetKeyUp(KeyCode.P))
            status = 0;
        if (Input.GetKeyDown(KeyCode.O) && status == 0)
        {
            status = 1;
            clients.Add(gameObject.AddComponent<ClientTchpConnect>());
        }
        else if (Input.GetKeyUp(KeyCode.O))
            status = 0;
        else if (Input.GetKeyDown(KeyCode.M) && status == 0)
        {
            
            status = 1;
            if (clients != null)
            {
                foreach (ClientTchpConnect client in clients)
                {
                    client.SendMessage("We need more minerals");
                }
            }
        }
        else if (Input.GetKeyUp(KeyCode.M))
            status = 0;
        else if (Input.GetKeyDown(KeyCode.A) && doOnce == false)
        {
            doOnce = true;
            Debug.Log("QUIITTTT");
            server.QuitAllClient();
            foreach (ClientTchpConnect cl in clients)
            {
                cl.CloseThread();
            }
            return;
            
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (IsClientAndServerInit != true)
        {
            server = GetComponent<ServerTcpConnect>();
            clients.Add(GetComponent<ClientTchpConnect>());
            IsClientAndServerInit = true;
        }
        if (test == false)
        {
            clients.Add(GetComponent<ClientTchpConnect>());
        }

    }
}
