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

        /// <summary>
        /// The PUN loglevel. 
        /// </summary>
        public PhotonLogLevel Loglevel = PhotonLogLevel.Informational;

        /// <summary>
        /// The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created.
        /// </summary>   
        [Tooltip("The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created")]
        public byte MaxPlayersPerRoom = 40;

        [Tooltip("The Ui Panel to let the user enter name, connect and play")]
        public GameObject controlPanel;
        [Tooltip("The Ui Panel to let the user enter name, connect and play")]
        public GameObject loginUsername;
        [Tooltip("The Ui Panel to let the user enter name, connect and play")]
        public GameObject loginPassword;

        [Tooltip("The Ui Panel to let the user enter name, connect and play")]
        public GameObject registrationPanel;
        [Tooltip("The Ui Panel to let the user enter name, connect and play")]
        public GameObject registrationUsername;
        [Tooltip("The Ui Panel to let the user enter name, connect and play")]
        public GameObject registrationPassword;
        [Tooltip("The Ui Panel to let the user enter name, connect and play")]
        public GameObject registrationEmail;

        [Tooltip("The UI Label to inform the user that the connection is in progress")]
        public GameObject progressLabel;

		public GameObject versionLabel;

		public GameObject errorLabel;

        public GameObject playername;

		public AudioClip clicksound;

		public AudioSource click;

        public GameObject loadingSpinner;
       

        
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

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during early initialization phase.
        /// </summary>
        void Awake() {
            // #Critical
            // we don't join the lobby. There is no need to join a lobby to get the list of rooms.
            PhotonNetwork.autoJoinLobby = false;


            // #Critical
            // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
            PhotonNetwork.automaticallySyncScene = true;

            // #NotImportant
            // Force LogLevel
            PhotonNetwork.logLevel = Loglevel;
        }


        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during initialization phase.
        /// </summary>
        void Start() {
            DepenencyInjector.SetupInjection();
            this.SetUserName();
            //discordApi = DepenencyInjector.Resolve<IDiscordApi>();
            progressLabel.SetActive(false);
            loadingSpinner.SetActive(false);
            controlPanel.SetActive(true);
            registrationPanel.SetActive(false);
            errorLabel.SetActive(false);
			versionLabel.GetComponent<UnityEngine.UI.Text>().text = "Version " + GameInfo.Version();
        }


        private void SetUserName()
        {
            var userController = DepenencyInjector.Resolve<IUserController>();
            PhotonNetwork.playerName = userController.GetWulframPlayerData().userName;
        }

        #endregion


        #region Public Methods

        /// <summary>
        /// Start the connection process. 
        /// - If already connected, we attempt joining a random room
        /// - if not yet connected, Connect this application instance to Photon Cloud Network
        /// </summary>
        public void Connect() {
            // keep track of the will to join a room, because when we come back from the game we will get a callback that we are connected, so we need to know what to do then
			click.PlayOneShot(clicksound, 0.3f);
            isConnecting = true;
            progressLabel.SetActive(true);
            loadingSpinner.SetActive(true);
            controlPanel.SetActive(true);
            //StartCoroutine(discordApi.PlayerJoined(PhotonNetwork.playerName));
            
            // we check if we are connected or not, we join if we are , else we initiate the connection to the server.
            if (PhotonNetwork.connected) {
				
                // #Critical we need at this point to attempt joining a Random Room. If it fails, we'll get notified in OnPhotonRandomJoinFailed() and we'll create one.
                PhotonNetwork.JoinRandomRoom();
            } else {
                // #Critical, we must first and foremost connect to Photon Online Server.
                PhotonNetwork.ConnectUsingSettings(GameInfo.Version());
            }
        }

        public async void Login()
        {
            progressLabel.SetActive(true);
            loadingSpinner.SetActive(true);
            controlPanel.SetActive(true);
            progressLabel.GetComponent<UnityEngine.UI.Text>().text = "Accessing governmental registery..";

            InputField userNameInputField = loginUsername.GetComponent<InputField>();
            InputField passwordInputField = loginPassword.GetComponent<InputField>();

            var user = await DepenencyInjector.Resolve<IUserController>().LoginUser(userNameInputField.text, passwordInputField.text);
            progressLabel.GetComponent<UnityEngine.UI.Text>().text = "Provisioning assests...";
            if (user != null)
            {
                Connect();
            }
            else
            {
                userNameInputField.text = "";
                passwordInputField.text = "";
                progressLabel.SetActive(false);
                loadingSpinner.SetActive(false);
                controlPanel.SetActive(true);
                // Login failed
                errorLabel.SetActive(true);
                //Get the GUIText Component attached to that GameObject named Best
                errorLabel.GetComponent<UnityEngine.UI.Text>().text = "Invalid username or password";
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

            controlPanel.SetActive(false);
            registrationPanel.SetActive(true);
        }

        public void CloseRegistrationPanel()
        {
            registrationPanel.SetActive(false);
            controlPanel.SetActive(true);

        }

        public async void RegisterNewUser()
        {
            progressLabel.SetActive(true);
            loadingSpinner.SetActive(true);
            registrationPanel.SetActive(false);
            progressLabel.GetComponent<UnityEngine.UI.Text>().text = "Validating new user...";

            InputField userNameInputField = registrationUsername.GetComponent<InputField>();
            InputField passwordInputField = registrationPassword.GetComponent<InputField>();
            InputField emailInputField = registrationEmail.GetComponent<InputField>();

            var message = await DepenencyInjector.Resolve<IUserController>().RegisterUser(userNameInputField.text, passwordInputField.text, emailInputField.text);
            progressLabel.GetComponent<UnityEngine.UI.Text>().text = "Creating new user....";

            progressLabel.SetActive(false);
            loadingSpinner.SetActive(false);
            CloseRegistrationPanel();
            errorLabel.SetActive(true);
            //Get the GUIText Component attached to that GameObject named Best
            errorLabel.GetComponent<UnityEngine.UI.Text>().text = message;
        }

        ////private void RegisterUserCompleted(string obj)
        ////{
        ////    DepenencyInjector.Resolve<IUserController>().RegisterUserCompleted -= RegisterUserCompleted;
            
        ////}

        public void Quit() {
            Application.Quit();
        }


        #endregion

        #region Photon.PunBehaviour CallBacks


        public override void OnConnectedToMaster() {


            Debug.Log("OnConnectedToMaster() called by PUN (LauncherWithLogin/OnConnectedToMaster:255)");
            // we don't want to do anything if we are not attempting to join a room. 
            // this case where isConnecting is false is typically when you lost or quit the game, when this level is loaded, OnConnectedToMaster will be called, in that case
            // we don't want to do anything.
            if (isConnecting) {
                // #Critical: The first we try to do is to join a potential existing room. If there is, good, else, we'll be called back with OnPhotonRandomJoinFailed()
                PhotonNetwork.JoinRandomRoom();
            }

        }


        public override void OnDisconnectedFromPhoton() {
            //Debug.Log("DemoAnimator/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");
            //string discordURI = "https://discordapp.com/api/webhooks/389264790230532107/LgvTNdOLb28JQmtTpK1yBzam-CMAnEhDqLkmXT4CqAyP-8id8ydWisx2yz8Ga6fQ5wX2";


            //string greetdiscord = string.Format ("{0} has disconnected from Wulfram 3!", PhotonNetwork.playerName);
            //string postdiscord = "{ \"content\": \"" + greetdiscord + "\" } ";
            //Debug.Log (postdiscord);
            
            //Debug.LogWarning("DemoAnimator/Launcher: OnDisconnectedFromPhoton() was called by PUN");
        }

        public override void OnPhotonRandomJoinFailed(object[] codeAndMsg) {
            // Debug.Log("DemoAnimator/Launcher:OnPhotonRandomJoinFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom(null, new RoomOptions() {maxPlayers = 4}, null);");
            Debug.Log("Creating Room. Max Players: " + MaxPlayersPerRoom + " (LauncherWithLogin/OnPhotonRandomJoinFailed:218)");
            // #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
            PhotonNetwork.CreateRoom(null, new RoomOptions() { MaxPlayers = MaxPlayersPerRoom }, null);
        }


        public override void OnJoinedRoom() {
          

            Debug.Log("Room Joined. (LauncherWithLogin/OnJoinedRoom:290)");
            // #Critical: We only load if we are the first player, else we rely on  PhotonNetwork.automaticallySyncScene to sync our instance scene.
            if (PhotonNetwork.room.PlayerCount == 1) {
                Debug.Log("Loading 'Playground'. (LauncherWithLogin/OnJoinedRoom:293)");
                // #Critical
                // Load the Room Level. 
                PhotonNetwork.LoadLevel("Playground");
            } else
            {
                Debug.Log("Syncing 'Playground'. (LauncherWithLogin/OnJoinedRoom:299)");
            }
        }

        public override void OnLeftRoom()
        {
            Debug.Log("OnLeftRoom! (LauncherWithLogin/OnLeftRoom:305)");
            base.OnLeftRoom();
        }


        #endregion


    }
}