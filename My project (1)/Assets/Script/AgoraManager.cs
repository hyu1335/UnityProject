using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using agora_gaming_rtc;

public class AgoraManager : MonoBehaviour
{
    public string appID;
    public static IRtcEngine mRtcEngine;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    void Start()
    {
        if (mRtcEngine == null)
        {
            mRtcEngine = IRtcEngine.GetEngine(appID);
        }
        mRtcEngine.SetMultiChannelWant(true);
        mRtcEngine.EnableVideo();
        mRtcEngine.EnableVideoObserver();

    }
    private void OnApplicationQuit()
    {
        IRtcEngine.Destroy();
    }
}
