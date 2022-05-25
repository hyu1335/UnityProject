using System.Collections;

using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

namespace Com.MyCompany.MyGame
{
    public class GameManager : MonoBehaviourPunCallbacks
    {
        [Tooltip("the prefab to use for representing the player")]
        public GameObject playerPrefab;
        public static GameManager Instance;

        void Start()
        {
            Instance = this; //게임메니저 싱글톤 구현을 위해 설정
            if (playerPrefab == null)
            {
                Debug.LogError("playerPrefab 오브젝트가 할당되어있지 않습니다");
            }
            else
            {
                if (PlayerManager.LocalPlayerInstance == null) //player 오브젝트가 생성되기 전일때 playermanager 스크립트의 awake문이 실행이 되지 않아서 PlayerManager.LocalPlayerInstance 가 널값을 가지게 됌
                {
                    Debug.LogFormat("We are Instantiating LocalPlayer from {0}", Application.loadedLevelName); //씬이름 표시
                    PhotonNetwork.Instantiate(this.playerPrefab.name, new Vector3(0f, 5f, 0f), Quaternion.identity, 0);
                }
                else
                {
                    Debug.LogFormat("Ignoring scene load for {0}", SceneManagerHelper.ActiveSceneName);
                }
            }
        }

        void LoadArena() //플레이어의 수에 맞는 씬을 다시 불러오는 함수
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.LogError("PhotonNetwork : Trying to Load a level but we are not the master Client");
            }
            Debug.LogFormat("PhotonNetwork : Loading Level : {0}", PhotonNetwork.CurrentRoom.PlayerCount);
            PhotonNetwork.LoadLevel("Room for " + PhotonNetwork.CurrentRoom.PlayerCount);
        }

        public override void OnPlayerEnteredRoom(Player other) //다른 플레이어 입장시
        {
            Debug.LogFormat("OnPlayerEnteredRoom() {0}", other.NickName);
            if (PhotonNetwork.IsMasterClient)
            {
                Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient);
                LoadArena();
            }
        }

        public override void OnPlayerLeftRoom(Player other) //다른 플레이어들이 방을 나갈때 자동으로 실행되는 콜백함수
        {
            Debug.LogFormat("OnPlayerLeftRoom() {0}", other.NickName);

            if (PhotonNetwork.IsMasterClient)
            {
                Debug.LogFormat("OnPlayerLeftRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient);
                LoadArena();
            }

        }

        public override void OnLeftRoom() //LeaveRoom실행시 자동으로 실행되는 콜백함수
        {
            SceneManager.LoadScene(0);
        }
        public void LeaveRoom()
        {
            PhotonNetwork.LeaveRoom();
        }
    }
}
