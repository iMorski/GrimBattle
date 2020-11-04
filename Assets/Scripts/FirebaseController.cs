using System.Collections.Generic;
using System.Linq;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;
using UnityEngine;

public class FirebaseController : MonoBehaviour
{
    [SerializeField] private string _Link;
    [SerializeField] private string _PrettyTextPlayer;
    [SerializeField] private string _PrettyTextRoom;
    [SerializeField] private int _KeyNumberCount;
    [SerializeField] private int _RoomCapacity;
    
    public static FirebaseApp BaseApp;
    
    private static DatabaseReference BaseReference;
    private static DatabaseReference BaseTracking;

    private static readonly System.Random Random = new System.Random();

    public static string MyName = "";
    public static string MyRoom = "";
    
    public static readonly Dictionary<string, string> MyData = new Dictionary<string, string>();
    public static readonly Dictionary<Dictionary<string, string>, string> InRoomData = new Dictionary<Dictionary<string, string>, string>();

    private static string Link;
    private static string PrettyTextPlayer;
    private static string PrettyTextRoom;
    private static int KeyNumberCount;
    private static int RoomCapacity;

    public static double ConnectionStep;

    public static bool OnRoomDataChange;
    public static bool OnConnect;

    public static void Check()
    {
        ConnectionStep = 0.0;
        Debug.Log("Step: 0.0 - Checking resources");
        
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(Task => 
        {
            if (Task.Result != DependencyStatus.Available) return;
            
            BaseApp = FirebaseApp.DefaultInstance;
            BaseApp.SetEditorDatabaseUrl(Link);
            
            FirebaseDatabase.DefaultInstance.GetReference(".info/connected").ValueChanged += CheckConnect;

            void CheckConnect(object Sender, ValueChangedEventArgs Argument)
            {
                if (Argument.Snapshot.Value.ToString() != "True")
                {
                    OnConnect = false;
                }
                else
                {
                    OnConnect = true;
                }
            }

            BaseReference = FirebaseDatabase.DefaultInstance.RootReference;
            
            ConnectionStep = 1.0;
            Debug.Log("Step: 1.0 - Checking done");
        });
    }
    
    private void Awake()
    {
        Link = _Link;
        PrettyTextPlayer = _PrettyTextPlayer;
        PrettyTextRoom = _PrettyTextRoom;
        KeyNumberCount = _KeyNumberCount;
        RoomCapacity = _RoomCapacity;
        
        Check();
    }
    
    private static string GenerateKey(string PrettyText)
    {
        string Key = PrettyText;
        
        for (int i = 0; i < KeyNumberCount; i++)
        {
            Key = Key + Random.Next(0, 10);
        }

        return Key;
    }

    private static bool Free(DataSnapshot Snapshot, string Key)
    {
        foreach (DataSnapshot Child in Snapshot.Children)
        {
            if (Child.Key != Key)
            {
                continue;
            }
                        
            return false;
        }
                    
        return true;
    }
    
    private static void Collect(DataSnapshot Room)
    {
        InRoomData.Clear();

        foreach (DataSnapshot Player in Room.Children)
        {
            Dictionary<string, string> DataDictionary = new Dictionary<string, string>();
                
            foreach (DataSnapshot Data in Player.Children)
            {
                DataDictionary.Add(Data.Key, Data.Value.ToString());
            }
            
            InRoomData.Add(DataDictionary, Player.Key);
        }

        OnRoomDataChange = true;
    }

    public static void Connect()
    {
        ConnectionStep = 2.0;
        Debug.Log("Step: 2.0 - Connecting");
        
        BaseReference.GetValueAsync().ContinueWith(Task =>
        {
            if (Task.IsFaulted) return;
                
            DataSnapshot ActivePlayer = Task.Result.Child("ActivePlayer");
            DataSnapshot ActiveRoom = Task.Result.Child("ActivePlayer");
            DataSnapshot Lobby = Task.Result.Child("Lobby");

            if (!(MyName != ""))
            {
                MyName = GenerateKey(PrettyTextPlayer);

                while (!Free(ActivePlayer, MyName))
                {
                    MyName = GenerateKey(PrettyTextPlayer);
                }
                
                Debug.Log($"Step: 2.0 - {MyName}");

                foreach (KeyValuePair<string, string> Data in MyData)
                {
                    BaseReference.Child("ActivePlayer").Child(MyName).Child(Data.Key).SetValueAsync(Data.Value);
                }
            }

            if (Lobby.Children.Count() + 1 >= RoomCapacity)
            {
                ConnectionStep = 4.0;
                Debug.Log("Step: 4.0 - Creating room");
                
                MyRoom = GenerateKey(PrettyTextRoom);

                while (!Free(ActiveRoom, MyRoom))
                {
                    MyRoom = GenerateKey(PrettyTextRoom);
                }
                
                Debug.Log($"Step: 4.0 - {MyRoom}");

                BaseTracking = BaseReference.Child("ActiveRoom").Child(MyRoom);
                BaseTracking.ValueChanged += OnRoomChange;
            }
            else
            {
                ConnectionStep = 3.0;
                Debug.Log("Step: 3.0 - Joining lobby");
                
                BaseTracking = BaseReference.Child("Lobby").Child(MyName);
                BaseTracking.ValueChanged += OnLobbyChange;
            }
        });
    }

    private static void OnLobbyChange(object Sender, ValueChangedEventArgs Argument)
    {
        if (!(ConnectionStep != 3.1) && !(Argument.Snapshot.Value != null))
        {
            ConnectionStep = 3.2;
            Debug.Log("Step: 3.2 - Receiving invitation");
            
            BaseTracking.ValueChanged -= OnLobbyChange;
            BaseTracking = BaseReference.Child("ActiveRoom");
            BaseTracking.ValueChanged += OnActiveRoomChange;
        }
        else if (ConnectionStep != 3.1)
        {
            foreach (KeyValuePair<string, string> Data in MyData)
            {
                BaseTracking.Child(Data.Key).SetValueAsync(Data.Value);
            }

            ConnectionStep = 3.1;
            Debug.Log("Step: 3.1 - Waiting in lobby");
        }
    }

    private static void OnActiveRoomChange(object Sender, ValueChangedEventArgs Argument)
    {
        string Search(DataSnapshot Snapshot, string Key)
        {
            string Room = "";
                        
            foreach (DataSnapshot Child in Snapshot.Children)
            {
                if (Child.Key != Key)
                {
                    Room = Search(Child, Key);

                    if (Room != "")
                    {
                        break;
                    }
                }
                else
                {
                    Room = Snapshot.Key;
                    
                    break;
                }
            }

            return Room;
        }
        
        ConnectionStep = 3.3;
        Debug.Log($"Step: 3.3 - Checking room list");
        
        DataSnapshot ActiveRoom = Argument.Snapshot;
        MyRoom = Search(ActiveRoom, MyName);

        if (MyRoom != "")
        {
            ConnectionStep = 3.4;
            Debug.Log($"Step: 3.4 - {MyRoom}");
            
            BaseTracking.ValueChanged -= OnActiveRoomChange;
            BaseTracking = BaseReference.Child("ActiveRoom").Child(MyRoom);
            BaseTracking.ValueChanged += OnRoomChange;
        }
    }

    private static void OnRoomChange(object Sender, ValueChangedEventArgs Argument)
    {
        DataSnapshot Room = Argument.Snapshot;
        
        if (Room.Children.Any())
        {
            if (ConnectionStep < 4 && ConnectionStep != 3.5)
            {
                ConnectionStep = 3.5;
                Debug.Log("Step: 3.5 - Waiting in room");
            }
            else if (ConnectionStep < 5 && ConnectionStep != 4.3)
            {
                ConnectionStep = 4.3;
                Debug.Log("Step: 4.3 - Waiting in room");
            }
            
            if (!(ConnectionStep != 5))
            {
                if (Room.Children.Count() < RoomCapacity)
                {
                    Disconnect();
                }
                else
                {
                    Collect(Room);
                }
            }
            else if (!(Room.Children.Count() != RoomCapacity))
            {
                Collect(Room);

                ConnectionStep = 5.0;
                Debug.Log("Step: 5.0 - Ready");
            }
        }
        else
        {
            ConnectionStep = 4.1;
            Debug.Log("Step: 4.1 - Checking lobby");
            
            BaseReference.Child("Lobby").GetValueAsync().ContinueWith(Task =>
            {
                if (Task.IsFaulted) return;
                
                DataSnapshot Lobby = Task.Result;
                
                foreach (KeyValuePair<string, string> Data in MyData)
                {
                    BaseTracking.Child(MyName).Child(Data.Key).SetValueAsync(Data.Value);
                }

                for (int i = 1; i < RoomCapacity; i++)
                {
                    string LastName = Lobby.Children.ElementAt(Lobby.Children.Count() - i).Key;
                    
                    foreach (DataSnapshot Data in Lobby.Child(LastName).Children)
                    {
                        BaseTracking.Child(LastName).Child(Data.Key).SetValueAsync(Data.Value);
                    }
                
                    ConnectionStep = 4.2;
                    Debug.Log($"Step: 4.2 - Inviting {LastName}");
                    
                    BaseReference.Child("Lobby").Child(LastName).SetValueAsync(null);
                }
            });
        }
    }

    public static void Write()
    {
        foreach (KeyValuePair<string, string> Data in MyData)
        {
            BaseReference.Child("ActivePlayer").Child(MyName).Child(Data.Key).SetValueAsync(Data.Value);

            if (MyRoom != "")
            {
                BaseReference.Child("ActiveRoom").Child(MyRoom).Child(MyName).Child(Data.Key).SetValueAsync(Data.Value);
            }
        }
    }
    
    public static void Disconnect()
    {
        if (MyName != "")
        {
            if (MyRoom != "")
            {
                if (BaseTracking != null)
                {
                    BaseTracking.ValueChanged -= OnRoomChange;
                }
                
                BaseReference.Child("ActiveRoom").Child(MyRoom).Child(MyName).SetValueAsync(null);
                
                MyRoom = "";
                
                InRoomData.Clear();
            }
            else
            {
                if (BaseTracking != null)
                {
                    BaseTracking.ValueChanged -= OnLobbyChange;
                    BaseTracking.ValueChanged -= OnActiveRoomChange;
                }

                BaseReference.Child("Lobby").Child(MyName).SetValueAsync(null);
            }
            
            BaseReference.Child("ActivePlayer").Child(MyName).SetValueAsync(null);

            MyName = "";
        }

        ConnectionStep = 0;
    }
}