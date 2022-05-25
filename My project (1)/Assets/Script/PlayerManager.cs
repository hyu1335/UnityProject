using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace Com.MyCompany.MyGame
{
    public class PlayerManager : MonoBehaviourPunCallbacks,IPunObservable
    {
        [Tooltip("유저의 hp")]
        public float Health = 1f;
        [Tooltip("The Beams GameObject to control")]
        [SerializeField] private GameObject beams;
        public static GameObject LocalPlayerInstance;

        bool IsFiring;

        // Start is called before the first frame update
        void Awake() //PhotonNetwork.Instantiate 실행 후에 오브젝트가 생성되고 awake문이 시작된다
        {
            if (photonView.IsMine) //서버에 연결됐을때
            {
                LocalPlayerInstance = this.gameObject; 
            }
            DontDestroyOnLoad(this.gameObject); //새 씬을 불러올때 이 오브젝트를 삭제하지 마시오
            beams.SetActive(false);
        }
        void Start()
        {
            CameraWork _cameraWork = this.gameObject.GetComponent<CameraWork>();
            if (_cameraWork != null)
            {
                if (photonView.IsMine)
                {
                    _cameraWork.OnStartFollowing();
                }
            }
            else
            {
                Debug.LogError("CameraWork 컴포넌트가 존재하지 않습니다");
            }
        }
        // Update is called once per frame
        void Update()
        {
            if (photonView.IsMine) //내 캐릭터만 나의 키입력을 받는다
            {
                ProcessInputs();
            }

            if (beams != null && IsFiring != beams.activeInHierarchy) //beams.activeInHierarchy beam 오브젝트가 씬에 존재할때는 true, 아니면 false
            {
                beams.SetActive(IsFiring);
            }

            if (Health <= 0f)
            {
                GameManager.Instance.LeaveRoom();
            }
            
        }

        void ProcessInputs()
        {
            if (Input.GetButtonDown("Fire1"))
            {
                if (!IsFiring)
                {
                    IsFiring = true;
                }
            }
            if (Input.GetButtonUp("Fire1"))
            {
                if (IsFiring)
                {
                    IsFiring = false;
                }
            }
        }

        void OnTriggerEnter(Collider other) //다른 콜라이더에 충돌하기 시작한 순간에 자동 호출됌
        {
            if (!photonView.IsMine) //내 캐릭터일때만 true반환
            {
                return;
            }
            if (!other.name.Contains("Beam")) //부딫친 콜라이더의 이름이 빔일때 contains 함수는 true를 반환
            {
                return;
            }
            Health -= 0.1f;
        }

        void OnTriggerStay(Collider other)
        {
            if (!photonView.IsMine)
            {
                return;
            }

            if (!other.name.Contains("Beam"))
            {
                return;
            }
            Health -= 0.1f * Time.deltaTime;
        }
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) //photonview 컴포넌트에 등록되면 자동으로 호출되는 함수
        {
            if (stream.IsWriting) //stream.IsWriting 은 photonView.IsMine = true 일때만 true가 될 수 있다. + 내 캐릭터를 동작시켜서 정보가 입력될때만 true가 된다
            {
                stream.SendNext(Health);
                stream.SendNext(IsFiring); //내 동작 정보를 다른 플레이어에게 전달
            }
            else 
            {
                this.Health = (float)stream.ReceiveNext();
                this.IsFiring = (bool)stream.ReceiveNext(); //다른 플레이어의 IsFiring 변수값을 받아서 내 씬에 있는 다른 플레이어 오브젝트(this) 의 값으로 등록한다
            }
        }
    }
}
