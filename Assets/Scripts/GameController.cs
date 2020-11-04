using UnityEngine;

public class GameController : MonoBehaviour
{    
    private double ConnectionStep;
    
    private bool OnPauseSkip;

    private void Start()
    {
        FirebaseController.MyData.Add("Position", "0 : 0");
    }

    private void Update()
    {
        if (ConnectionStep != 1.0 && !(FirebaseController.ConnectionStep != 1.0))
        {
            FirebaseController.Connect();
            
            ConnectionStep = 1.0;
        }
    }

    #if UNITY_EDITOR

        private void OnApplicationQuit()
        {
            FirebaseController.Disconnect();
        }

    #else
    
        private void OnApplicationPause(bool OnPause)
        {
            if (OnPause && OnPauseSkip)
            {
                FirebaseController.Disconnect();
            }
            else
            {
                OnPauseSkip = true;
            }
        }
        
    #endif
}
