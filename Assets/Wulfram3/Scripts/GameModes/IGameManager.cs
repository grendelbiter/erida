using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGameManager
{
    
    string MapName { get; set;}

    int GameTime { get; set; }

    int SpawnTimeout { get; set; }

    int PlayerCountForStats { get; set; }

    bool AllowShipSpawn { get; set; }

}