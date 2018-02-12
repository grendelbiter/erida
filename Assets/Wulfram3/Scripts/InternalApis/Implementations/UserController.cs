using Assets.Wulfram3.Scripts.InternalApis.Classes;
using Assets.Wulfram3.Scripts.InternalApis.Interfaces;
using Newtonsoft.Json;
using socket.io;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Wulfram3.Scripts.InternalApis.Implementations
{
    public class UserController : IUserController
    {
        private Socket socketServer;
        private Api api;
        public UserController()
        {
            player = new WulframPlayer();

            Logger.Log("UserController constructor:" + player.userName);
            
            api = new Api();
        }

        private WulframPlayer player;

        public WulframPlayer GetWulframPlayerData()
        {
            if(string.IsNullOrEmpty(player.userName))
            {
                this.player.userName = GetUsername();
            }
            return player;
        }

        public async Task<WulframPlayer> LoginUser(string username, string password)
        {
            var result = await api.Login(username, password);
            if(result.message == "Login Complete!")
            {
                this.player = result.Result;
                SetupSocketConnection();
                return this.player;
            }

            return null;
        }

        public async Task<bool> LogoutUser()
        {
            var result = await api.Logout(this.player._id);
            return result.Result;
        }

        public async Task<string> RegisterUser(string username, string password, string email)
        {
            var result = await api.Register(username, password, email);
            return result.message;
        }

        public void RecordUnitKill(UnitType type)
        {
            this.RecordKill(type, 1);
            this.UpdatePlayer();
        }

        public void RecordUnitDeploy(UnitType type)
        {
            this.RecordDeploy(type, 1);
            this.UpdatePlayer();
        }

        public void RecordPlayerDeath(UnitType type)
        {
            this.RecordDeath(type);
            this.UpdatePlayer();
        }

        public void UpdateUserData()
        {
            this.UpdatePlayer();
        }

        private string GetUsername()
        {
            PlayerPrefs.DeleteAll();
            string defaultName = "";
            Logger.Log("defaultName:" + defaultName);

            var userString = this.player.userName;
            if (userString != "null")
            {
                // Auth'ed User
                switch (this.player.type)
                {
                    case "Moderator":
                        defaultName = "[MOD] " + userString;
                        break;

                    case "Developer":
                        defaultName = "[DEV] " + userString;
                        break;
                    default:
                        defaultName = userString;
                        break;
                }

                Logger.Log("defaultName:" + defaultName);
            }
            else
            {
                if (PlayerPrefs.HasKey("PlayerName"))
                {
                    defaultName = PlayerPrefs.GetString("PlayerName");
                    Logger.Log("defaultName:" + defaultName);
                }
                else
                {
                    defaultName = "GuestUser#" + new System.Random().Next(1, 9000);
                    Logger.Log("defaultName:" + defaultName);
                }
            }

            Logger.Log("defaultName:" + defaultName);
            PhotonNetwork.playerName = defaultName;
            return defaultName;
        }

        private void SetupSocketConnection()
        {
            socketServer = Socket.Connect(this.api.Url);

            socketServer.On(SystemEvents.connect, () => {
                Logger.Log("Hello, Socket.io~");
                socketServer.EmitJson("handshake", JsonConvert.SerializeObject(this.player));
            });

            socketServer.On("handshake", (string data) => {
                socketServer.EmitJson("handshake", JsonConvert.SerializeObject(this.player));
            });

            socketServer.On(SystemEvents.reconnect, (int reconnectAttempt) => {
                Logger.Log("Hello, Again! " + reconnectAttempt);
            });

            socketServer.On(SystemEvents.disconnect, () => {
                Logger.Log("Bye~");
            });
        }

        private void RecordKill(UnitType type, int value)
        {
            switch (type)
            {
                case UnitType.None:
                    break;
                case UnitType.Tank:
                    this.player.scores.tankKills += value;
                    break;
                case UnitType.Scout:
                    this.player.scores.scoutKills += value;
                    break;
                case UnitType.Cargo:
                    this.player.scores.cargoKills += value;
                    break;
                case UnitType.PowerCell:
                    this.player.scores.powercellKills += value;
                    break;
                case UnitType.RepairPad:
                    this.player.scores.repairpadKills += value;
                    break;
                case UnitType.RefuelPad:
                    this.player.scores.refullpadKills += value;
                    break;
                case UnitType.FlakTurret:
                    this.player.scores.flakturretKills += value;
                    break;
                case UnitType.GunTurret:
                    this.player.scores.gunturretKills += value;
                    break;
                case UnitType.MissleLauncher:
                    this.player.scores.misslelauncherKills += value;
                    break;
                case UnitType.Skypump:
                    this.player.scores.skypumpKills += value;
                    break;
                case UnitType.Darklight:
                    this.player.scores.darklightKills += value;
                    break;
                default:
                    break;
            }
        }

        private void RecordDeploy(UnitType type, int value)
        {
            switch (type)
            {
                case UnitType.PowerCell:
                    this.player.scores.powercellDeployed += value;
                    break;
                case UnitType.RepairPad:
                    this.player.scores.repairpadDeployed += value;
                    break;
                case UnitType.RefuelPad:
                    this.player.scores.refullpadDeployed += value;
                    break;
                case UnitType.FlakTurret:
                    this.player.scores.flakturretDeployed += value;
                    break;
                case UnitType.GunTurret:
                    this.player.scores.gunturretDeployed += value;
                    break;
                case UnitType.MissleLauncher:
                    this.player.scores.misslelauncherKills += value;
                    break;
                case UnitType.Skypump:
                    this.player.scores.skypumpDeployed += value;
                    break;
                case UnitType.Darklight:
                    this.player.scores.darklightDeployed += value;
                    break;
                default:
                    break;
            }
        }

        private void RecordDeath(UnitType type)
        {
            switch (type)
            {
                case UnitType.Tank:
                    this.player.scores.tankDeaths += 1;
                    break;
                case UnitType.Scout:
                    this.player.scores.scoutDeaths += 1;
                    break;
                default:
                    break;
            }
        }

        private void UpdatePlayer()
        {
            socketServer.EmitJson("updatePlayer", JsonConvert.SerializeObject(this.player));
        }
    }
}

public class Loom : MonoBehaviour
{
    public static int maxThreads = 8;
    static int numThreads;

    private static Loom _current;
    private int _count;
    public static Loom Current
    {
        get
        {
            Initialize();
            return _current;
        }
    }

    void Awake()
    {
        _current = this;
        initialized = true;
    }

    static bool initialized;

    static void Initialize()
    {
        if (!initialized)
        {

            if (!Application.isPlaying)
                return;
            initialized = true;
            var g = new GameObject("Loom");
            _current = g.AddComponent<Loom>();
        }

    }

    private List<Action> _actions = new List<Action>();
    public struct DelayedQueueItem
    {
        public float time;
        public Action action;
    }
    private List<DelayedQueueItem> _delayed = new List<DelayedQueueItem>();

    List<DelayedQueueItem> _currentDelayed = new List<DelayedQueueItem>();

    public static void QueueOnMainThread(Action action)
    {
        QueueOnMainThread(action, 0f);
    }
    public static void QueueOnMainThread(Action action, float time)
    {
        if (time != 0)
        {
            lock (Current._delayed)
            {
                Current._delayed.Add(new DelayedQueueItem { time = Time.time + time, action = action });
            }
        }
        else
        {
            lock (Current._actions)
            {
                Current._actions.Add(action);
            }
        }
    }

    public static Thread RunAsync(Action a)
    {
        Initialize();
        while (numThreads >= maxThreads)
        {
            Thread.Sleep(1);
        }
        Interlocked.Increment(ref numThreads);
        ThreadPool.QueueUserWorkItem(RunAction, a);
        return null;
    }

    private static void RunAction(object action)
    {
        try
        {
            ((Action)action)();
        }
        catch
        {
        }
        finally
        {
            Interlocked.Decrement(ref numThreads);
        }

    }


    void OnDisable()
    {
        if (_current == this)
        {

            _current = null;
        }
    }



    // Use this for initialization
    void Start()
    {

    }

    List<Action> _currentActions = new List<Action>();

    // Update is called once per frame
    void Update()
    {
        lock (_actions)
        {
            _currentActions.Clear();
            _currentActions.AddRange(_actions);
            _actions.Clear();
        }
        foreach (var a in _currentActions)
        {
            a();
        }
        lock (_delayed)
        {
            _currentDelayed.Clear();
            _currentDelayed.AddRange(_delayed.Where(d => d.time <= Time.time));
            foreach (var item in _currentDelayed)
                _delayed.Remove(item);
        }
        foreach (var delayed in _currentDelayed)
        {
            delayed.action();
        }



    }
}