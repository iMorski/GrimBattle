using System.Collections.Generic;
using UnityEngine;

public class LC : MonoBehaviour
{
    private bool OnPauseSkip;

    private void Start()
    {
        FB.MyData.Add("Nick", "");

        FB.ConnectionStepChange += OnConnectionStepChange;
        FB.RoomDataChange += OnRoomDataChange;
    }

    private void OnConnectionStepChange()
    {
        if (!(FB.ConnectionStep != 1.0))
        {
            FB.Connect();
        }
    }

    private void OnRoomDataChange(Dictionary<Dictionary<string, string>, string> RoomData)
    {
        
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
