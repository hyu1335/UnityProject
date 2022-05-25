using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using agora_gaming_rtc;

public class ChannelPanel : MonoBehaviour
{
    [SerializeField] private string channelName;
    [SerializeField] private string channelToken;

    [SerializeField] private Transform videoSpawnPoint;
    [SerializeField] private RectTransform panelContentWindow;
    [SerializeField] private bool isPublishing;

    private AgoraChannel newChannel;
    private List<GameObject> userVideos;

    private const float SPACE_BETWEEN_USER_VIDEOS = 150f;

    void Start()
    {
        userVideos = new List<GameObject>();
    }

    void MakeImageSurface(string channelID, uint uid, bool isLocalUser = false)
    {
        if (GameObject.Find(uid.ToString()) != null) //오브젝트 리스트에 해당 유저의 이름을 가진 오브젝트가 있을때
        {
            Debug.Log("Already created videoSurface");
            return;
        }
        GameObject go = new GameObject();
        go.name = uid.ToString();
        RawImage userVideo = go.AddComponent<RawImage>();
        go.transform.localScale = new Vector3(1, 1, 1);

        if (videoSpawnPoint != null)
        {
            go.transform.SetParent(videoSpawnPoint);
        }
        panelContentWindow.sizeDelta = new Vector2(userVideos.Count * SPACE_BETWEEN_USER_VIDEOS, 0);
        float spawnX = userVideos.Count * SPACE_BETWEEN_USER_VIDEOS;
        userVideos.Add(go);

        go.GetComponent<RectTransform>().anchoredPosition = new Vector2(spawnX,0);

        VideoSurface videoSurface = go.AddComponent<VideoSurface>();
        if (isLocalUser == false)
        {
            videoSurface.SetForMultiChannelUser(channelID, uid);
        }
    }

    void UpdatePlayerVideoPositions()
    {
        for (int i = 0; i < userVideos.Count; i++)
        {
            userVideos[i].GetComponent<RectTransform>().anchoredPosition = Vector2.right * SPACE_BETWEEN_USER_VIDEOS*i;
        }
    }

    void RemoveUserVideoSurface(uint deletedUID)
    {
        foreach (GameObject player in userVideos)
        {
            if (player.name == deletedUID.ToString())
            {
                userVideos.Remove(player);
                Destroy(player);

                UpdatePlayerVideoPositions();
                Vector2 oldContent = panelContentWindow.sizeDelta;
                panelContentWindow.sizeDelta = oldContent + Vector2.left * SPACE_BETWEEN_USER_VIDEOS;
                break;
            }
        }
    }

    void OnApplicationQuit()
    {
        if (newChannel != null)
        {
            newChannel.LeaveChannel();
            newChannel.ReleaseChannel();
        }
    }

    public void OnJoinChannelSuccessHandler(string channelID, uint uid, int elapsed)
    {
        Debug.Log("Join Channel success, channel: " + channelID + "uid: " + uid);
        MakeImageSurface(channelID, uid, true);
    }

    public void OnUserJoinedHandler(string channelID, uint uid, int elapsed)
    {
        Debug.Log("uid: " + uid+ "joined channel");
    }

    public void OnLeaveHandler(string channelID, RtcStats stats)
    {
        Debug.Log("you left the channel");
        foreach (GameObject player in userVideos)
        {
            Destroy(player);
        }
        userVideos.Clear();
    }

    public void OnUserLeftHandler(string channelID, uint uid, USER_OFFLINE_REASON reason)
    {
        Debug.Log("user "+uid +"left channel for reason: " + reason);
        RemoveUserVideoSurface(uid);
    }

    private void OnRemoteVideoStatsHandler(string channelID, RemoteVideoStats remoteStats) //2초에 한번 모든 유저를 대상으로 호출됌
    {
        // If user is publishing...
        if (remoteStats.receivedBitrate > 0) //remoteStats.receivedBitrate가 0보다 클때만 비디오를 송출중이라고 인식함
        {
            bool needsToMakeNewImageSurface = true;
            foreach (GameObject user in userVideos)
            {
                if (remoteStats.uid.ToString() == user.name) //만약 해당 유저가 오브젝트 리스트에 등록되어있다면
                {
                    needsToMakeNewImageSurface = false; //그냥 넘어간다
                    break;
                }
            }
            // ... and their video surface isn't currently displaying ...
            if (needsToMakeNewImageSurface == true) //만약 송출중인데 유저리스트에 등록되지않아서 화면이 안띄워진 사람이 있으면 화면을 띄운다
            {
                // ... display their feed.
                MakeImageSurface(channelID, remoteStats.uid);
            }
        }
        // If user isn't publishing ...
        else if (remoteStats.receivedBitrate == 0) //비디오송출을 하지 않는다고 인식. 비디오화면을 삭제함
        {
            bool needsToRemoveUser = false;
            foreach (GameObject user in userVideos)
            {
                if (remoteStats.uid.ToString() == user.name) //만약 송출하는 데이터가 없는데 유저리스트에는 등록된 사람이 있다면
                {
                    needsToRemoveUser = true;
                }
            }

            if (needsToRemoveUser == true)
            {
                RemoveUserVideoSurface(remoteStats.uid); //해당 유저가 송출하는 화면을 제거한다
            }
        }
    }

    public void Join_Channel()
    {
        if (newChannel == null)
        {
            newChannel = AgoraManager.mRtcEngine.CreateChannel(channelName);

            newChannel.ChannelOnJoinChannelSuccess = OnJoinChannelSuccessHandler;
            newChannel.ChannelOnUserJoined = OnUserJoinedHandler;
            newChannel.ChannelOnLeaveChannel = OnLeaveHandler;
            newChannel.ChannelOnUserOffLine = OnUserLeftHandler;
            newChannel.ChannelOnRemoteVideoStats = OnRemoteVideoStatsHandler;

        }
        newChannel.JoinChannel(channelToken, null, 0, new ChannelMediaOptions(true, true));
        Debug.Log("Joining channel: " + channelName);
    }

    public void Leave_Channel()
    {
        if (newChannel != null)
        {
            newChannel.LeaveChannel();
            Debug.Log("Leaving Channel: " + channelName);
        }
    }
}
