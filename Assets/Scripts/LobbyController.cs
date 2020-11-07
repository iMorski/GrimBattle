using UnityEngine;

public class LobbyController : MonoBehaviour
{
    private double ConnectionStep;
    
    private bool OnPauseSkip;

    private void Start()
    {
        FB.MyData.Add("Nick", "");
    }

    private void Update()
    {
        if (ConnectionStep != 1.0 && !(FB.ConnectionStep != 1.0))
        {
            FB.Connect();
            
            ConnectionStep = 1.0;
        }
        
        if (ConnectionStep != 2.0 && (!(FB.ConnectionStep != 3.1) || !(FB.ConnectionStep != 4.2)) && FB.MyName != "")
        {
            FB.MyData["Nick"] = FB.MyName;
            
            if (PlayerPrefs.HasKey("Nick"))
            {
                FB.MyData["Nick"] = PlayerPrefs.GetString("Nick");
            }
            
            ConnectionStep = 2.0;
        }

        if (ConnectionStep != 3.0 && !(FB.ConnectionStep != 5.0))
        {
            FB.Write(FB.MyName, FB.MyData);
            
            ConnectionStep = 3.0;
        }

        if (FB.OnRoomDataChange)
        {
            FB.OnRoomDataChange = false;
        }
    }

    #if UNITY_EDITOR

        private void OnApplicationQuit()
        {
            FB.Disconnect();
        }

    #else
    
        private void OnApplicationPause(bool OnPause)
        {
            if (OnPause && OnPauseSkip)
            {
                FB.Disconnect();
            }
            else
            {
                OnPauseSkip = true;
            }
        }
        
    #endif
}
