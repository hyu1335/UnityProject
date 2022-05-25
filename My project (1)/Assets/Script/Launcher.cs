using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

namespace Com.Mycompany.MyGame
{
    public class Launcher : MonoBehaviourPunCallbacks
    {
        bool isConnecting;
        
        [SerializeField] private byte maxPlayerPerRoom = 4;
        [SerializeField] private GameObject controlPanel;
        [SerializeField] private GameObject progressLabel;

        string gameVersion = "1";

        void Awake()
        {
            
            PhotonNetwork.AutomaticallySyncScene = true;
        }

        void Start()
        {
            //Connect();
            
            progressLabel.SetActive(false);
            controlPanel.SetActive(true);
        }

        public void Connect()
        {
            progressLabel.SetActive(true);
            controlPanel.SetActive(false);
            isConnecting = true;

            if (PhotonNetwork.IsConnected) //마스터서버에 연결되있을때 true반환
            {
                PhotonNetwork.JoinRandomRoom();
            }
            else
            {
                
                PhotonNetwork.GameVersion = gameVersion;
                PhotonNetwork.ConnectUsingSettings();
                
            }
        }

        public override void OnConnectedToMaster() //PhotonNetwork.ConnectUsingSettings() 실행되면 자동으로 실행
        {
            Debug.Log("PUN Basics Tutorial/Launcher: OnConnectedToMaster() was called by PUN");
            if(isConnecting)
            {
                PhotonNetwork.JoinRandomRoom();
            }
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            progressLabel.SetActive(false);
            controlPanel.SetActive(true);
            Debug.LogWarningFormat("PUN Basics Tutorial/Launcher: OnDisconnected() was called by PUN with reason {0}", cause);
        }
        public override void OnJoinRandomFailed(short returnCode, string message) //PhotonNetwork.JoinRandomRoom() 실패시 자동으로 실행
        {
            Debug.Log("PUN Basics Tutorial/Launcher:OnJoinRandomFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");
            PhotonNetwork.CreateRoom(null, new RoomOptions() { MaxPlayers=maxPlayerPerRoom});
        }
        public override void OnJoinedRoom() //PhotonNetwork.CreateRoom() 성공시 자동으로 실행
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
            {
                Debug.Log("We load the 'Room for 1' ");
                PhotonNetwork.LoadLevel("Room for 1");
            }
            Debug.Log("PUN Basics Tutorial/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");
            
        }
    }
}
