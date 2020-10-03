using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Data;

public class NetworkMan : MonoBehaviour
{
    public UdpClient udp;
    public GameObject[] CubeSpawn;
    public GameObject player;
    // Start is called before the first frame update
    void Start()
    {
        //GameObject.CreatePrimitive(PrimitiveType.Cube);

        udp = new UdpClient();
        
        udp.Connect("18.222.83.238", 12345);

        Byte[] sendBytes = Encoding.ASCII.GetBytes("connect");
      
        udp.Send(sendBytes, sendBytes.Length);

        udp.BeginReceive(new AsyncCallback(OnReceived), udp);

        InvokeRepeating("HeartBeat", 1, 1);
    }

    void OnDestroy(){
        udp.Dispose();
    }


    public enum commands{
        NEW_CLIENT,
        UPDATE,
        CALLPLAYER
    };
    
    [Serializable]
    public class Message{
        public commands cmd;
    }
    
    [Serializable]
    public class Player{
        [Serializable]
        public struct receivedColor{
            public float R;
            public float G;
            public float B;
        }
        public string id;
        public receivedColor color;        
    }

    [Serializable]
    public class NewPlayer{
        
    }

    [Serializable]
    public class GameState{
        public Player[] players;
    }

    [Serializable]
    public class Playeradd
    {
        public Player player;
    }

    public GameState OtherPlayers;
    public Playeradd FirstPlayer;
    public Message latestMessage;
    public GameState lastestGameState;
    void OnReceived(IAsyncResult result){
        // this is what had been passed into BeginReceive as the second parameter:
        UdpClient socket = result.AsyncState as UdpClient;
        
        // points towards whoever had sent the message:
        IPEndPoint source = new IPEndPoint(0, 0);

        // get the actual message and fill out the source:
        byte[] message = socket.EndReceive(result, ref source);
        
        // do what you'd like with `message` here:
        string returnData = Encoding.ASCII.GetString(message);
        Debug.Log("Got this: " + returnData);
        
        latestMessage = JsonUtility.FromJson<Message>(returnData);
        try{
            switch(latestMessage.cmd){
                case commands.NEW_CLIENT:
                    FirstPlayer = JsonUtility.FromJson<Playeradd>(returnData);
                    Debug.Log(FirstPlayer.player.id);
                    Debug.Log("Laji");
                    break;
                case commands.UPDATE:
                    lastestGameState = JsonUtility.FromJson<GameState>(returnData);
                    break;
                case commands.CALLPLAYER:
                    OtherPlayers = JsonUtility.FromJson<GameState>(returnData);
                    break;
                default:
                    Debug.Log("Error");
                    break;
            }
        }
        catch (Exception e){
            Debug.Log(e.ToString());
        }
        
        // schedule the next receive operation once reading is done:
        socket.BeginReceive(new AsyncCallback(OnReceived), socket);
    }

    void SpawnPlayers(){

        if (FirstPlayer.player.id.Length > 0)
        {
            foreach (GameObject cube in GameObject.FindGameObjectsWithTag("Player"))
            {
                if (cube.GetComponent<PlayID>().AddrID == FirstPlayer.player.id)
                {
                    return;
                }
            }

            int PlayerSpawn = UnityEngine.Random.Range(0, 5);
            GameObject CubeObj;
            CubeObj = Instantiate(player, CubeSpawn[PlayerSpawn].transform.position, Quaternion.identity);
            CubeObj.GetComponent<PlayID>().AddrID = FirstPlayer.player.id;
        }
        foreach (Player P in OtherPlayers.players)
        {
            foreach(GameObject cube in GameObject.FindGameObjectsWithTag("Player"))
            {
                if(cube.GetComponent<PlayID>().AddrID == P.id)
                {
                    return;
                }
            }
           
            int PlayerSpawn = UnityEngine.Random.Range(0, 5);
            GameObject CubeObj;
            CubeObj = Instantiate(player, CubeSpawn[PlayerSpawn].transform.position, Quaternion.identity);
            CubeObj.GetComponent<PlayID>().AddrID = P.id;
        }

    }

    void UpdatePlayers(){
        
        

    }//RGB

    void DestroyPlayers(){


    }
    
    void HeartBeat(){
        Byte[] sendBytes = Encoding.ASCII.GetBytes("heartbeat");
        udp.Send(sendBytes, sendBytes.Length);
    }

    void Update(){
        SpawnPlayers();
        UpdatePlayers();
        DestroyPlayers();
    }
}
