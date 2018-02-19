using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;
using System.Text;
using System.Net;
using Assets.Wulfram3.Scripts.InternalApis;
using Assets.Wulfram3.Scripts.InternalApis.Classes;
using Assets.Wulfram3.Scripts.InternalApis.Interfaces;
using Assets.Wulfram3.Scripts.InternalApis.Implementations;
using UnityEngine.Experimental.UIElements;
using System;

namespace Com.Wulfram3 {
    public class LauncherWithLogin : Photon.PunBehaviour {
        #region Public Variables

        public PhotonLogLevel Loglevel = PhotonLogLevel.ErrorsOnly;

        public byte MaxPlayersPerRoom = 40;

        public GameObject loginPanel;
        public GameObject loadingPanel;
        public GameObject registrationPanel;
        public InputField loginUsername;
        public InputField loginPassword;
        public InputField registrationUsername;
        public InputField registrationPassword;
        public InputField registrationEmail;

        public Text progressLabel;
		public Text versionLabel;
		public Text errorLabel;
        public Text playername;

		public AudioClip clicksound;
		public AudioSource click;
        public GameObject loadingSpinner;

        private float errorDisplayTime;

        enum LauncherPanels { Login, Loader, Register};
        #endregion

        #region Private Variables

        /// <summary>
        /// Keep track of the current process. Since connection is asynchronous and is based on several callbacks from Photon, 
        /// we need to keep track of this to properly adjust the behavior when we receive call back by Photon.
        /// Typically this is used for the OnConnectedToMaster() callback.
        /// </summary>
        bool isConnecting;

        IDiscordApi discordApi;
        #endregion


        #region MonoBehaviour CallBacks

        void Awake() {
            // #Critical
            // we don't join the lobby. There is no need to join a lobby to get the list of rooms.
            PhotonNetwork.autoJoinLobby = false;
            // #Critical
            // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
            PhotonNetwork.automaticallySyncScene = true;
            //PhotonNetwork.logLevel = Loglevel; [ This should be set in PhotonServerSettings]
        }


        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during initialization phase.
        /// </summary>
        async void Start() {
            var api = new Api();
            await api.Startup();
            DepenencyInjector.SetupInjection();
            SetCurrentPanel(LauncherPanels.Login);
			versionLabel.text = "Version " + GameInfo.Version();
        }

        private void Update()
        {
            if (errorLabel.text != "" && Time.time > errorDisplayTime)
                errorLabel.text = "";
            if (loadingPanel.GetActive())
                loadingSpinner.transform.Rotate(Vector3.forward, -4f);
        }

        #endregion


        #region Public Methods

        /// <summary>
        /// Start the connection process. 
        /// - If already connected, we attempt joining a random room
        /// - if not yet connected, Connect this application instance to Photon Cloud Network
        /// </summary>
        public void Connect() {
            SetCurrentPanel(LauncherPanels.Loader);
            click.PlayOneShot(clicksound, 0.3f);
            // keep track of the will to join a room, because when we come back from the game we will get a callback that we are connected, so we need to know what to do then
            isConnecting = true;          
            // we check if we are connected or not, we join if we are , else we initiate the connection to the server.
            if (PhotonNetwork.connected)
                PhotonNetwork.JoinRandomRoom();// #Critical we need at this point to attempt joining a Random Room. If it fails, we'll get notified in OnPhotonRandomJoinFailed() and we'll create one.
            else
                PhotonNetwork.ConnectUsingSettings(GameInfo.Version()); // #Critical, we must first and foremost connect to Photon Online Server.
        }

        public async void Login()
        {
            SetCurrentPanel(LauncherPanels.Loader);
            progressLabel.text = "Accessing governmental registry..";
            InputField userNameInputField = loginUsername.GetComponent<InputField>();
            InputField passwordInputField = loginPassword.GetComponent<InputField>();
            var user = await DepenencyInjector.Resolve<IUserController>().LoginUser(userNameInputField.text, passwordInputField.text);
            progressLabel.text = "Provisioning assests...";
            if (user != null)
            {
                Connect();
            }
            else
            {
                loginUsername.text = "";
                loginPassword.text = "";
                SetCurrentPanel(LauncherPanels.Login);
                PostErrorToUser("Invalid username or password");
            }
        }


        public void OpenRegistrationPanel()
        {
            InputField userNameInputField = registrationUsername.GetComponent<InputField>();
            InputField passwordInputField = registrationPassword.GetComponent<InputField>();
            InputField emailInputField = registrationEmail.GetComponent<InputField>();
            userNameInputField.text = "";
            passwordInputField.text = "";
            emailInputField.text = "";
            SetCurrentPanel(LauncherPanels.Register);
        }

        public void CloseRegistrationPanel()
        {
            SetCurrentPanel(LauncherPanels.Login);
        }

        public async void RegisterNewUser()
        {
            SetCurrentPanel(LauncherPanels.Loader);
            progressLabel.text = "Validating new user...";
            InputField userNameInputField = registrationUsername.GetComponent<InputField>();
            InputField passwordInputField = registrationPassword.GetComponent<InputField>();
            InputField emailInputField = registrationEmail.GetComponent<InputField>();
            var message = await DepenencyInjector.Resolve<IUserController>().RegisterUser(userNameInputField.text, passwordInputField.text, emailInputField.text);
            progressLabel.text = "Creating new user....";
            SetCurrentPanel(LauncherPanels.Login);
            PostErrorToUser(message);
        }

        public void Quit() {
            Application.Quit();
        }

        private void SetCurrentPanel(LauncherPanels lp)
        {
            if (lp == LauncherPanels.Login)
            {
                loginPanel.SetActive(true);
                loadingPanel.SetActive(false);
                registrationPanel.SetActive(false);
            }
            else if (lp == LauncherPanels.Loader)
            {
                loginPanel.SetActive(false);
                loadingPanel.SetActive(true);
                registrationPanel.SetActive(false);
            }
            else if (lp == LauncherPanels.Register)
            {
                loginPanel.SetActive(false);
                loadingPanel.SetActive(false);
                registrationPanel.SetActive(true);
            }
        }

        public void PostErrorToUser(string s)
        {
            errorDisplayTime = Time.time + 3f;
            errorLabel.text = s;
        }
        #endregion

        #region Photon.PunBehaviour CallBacks

        public override void OnConnectedToMaster() {
            Logger.Log("Connected to MasterClient. OnConnectedToMaster() attempting to join existing room.");
            if (isConnecting)
                PhotonNetwork.JoinRandomRoom();
        }

        public override void OnDisconnectedFromPhoton() {
            //Logger.Log("DemoAnimator/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");
            //string discordURI = "https://discordapp.com/api/webhooks/389264790230532107/LgvTNdOLb28JQmtTpK1yBzam-CMAnEhDqLkmXT4CqAyP-8id8ydWisx2yz8Ga6fQ5wX2";
            //string greetdiscord = string.Format ("{0} has disconnected from Wulfram 3!", PhotonNetwork.playerName);
            //string postdiscord = "{ \"content\": \"" + greetdiscord + "\" } ";
            //Logger.Log (postdiscord);
            //Logger.Warning("DemoAnimator/Launcher: OnDisconnectedFromPhoton() was called by PUN");
        }

        public override void OnPhotonRandomJoinFailed(object[] codeAndMsg) {
            Logger.Log("No rooms exist or all rooms are full. OnPhotonRandomJoinFailed() is creating a room with a maximum player count of " + MaxPlayersPerRoom);
            PhotonNetwork.CreateRoom(null, new RoomOptions() { MaxPlayers = MaxPlayersPerRoom }, null);
        }


        public override void OnJoinedRoom() {
            if (PhotonNetwork.room.PlayerCount == 1) {
                Logger.Log("You are MasterClient. OnJoinedRoom() is Loading 'Playground'.");
                PhotonNetwork.LoadLevel("Playground");
            } else
            {
                Logger.Log("Found Existing Room. OnJoinedRoom() will connect to existing 'Playground'.");
            }
        }

        public override void OnLeftRoom()
        {
            Logger.Log("OnLeftRoom()");
            base.OnLeftRoom();
        }

        #endregion


    }
}