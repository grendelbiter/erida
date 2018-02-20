﻿using Assets.Wulfram3.Scripts.HUD;
using Assets.Wulfram3.Scripts.InternalApis.Classes;
using Assets.Wulfram3.Scripts.Units;
using Com.Wulfram3;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapModeManager : MonoBehaviour {

    public GameObject MasterHUD;
    public GameObject MapModeHUD;
    public GameObject SpawnModeHUD;

    public GameObject MapCenter;
    public GameObject SpawnCenter;

    public KGFMapSystem itsMapSystem;

    private MapType currentMapType;
    // Use this for initialization
    void Start () {
        this.ActivateMapMode(MapType.Spawn);
        itsMapSystem.EventMouseMapIconClicked += OnMapMarkerClicked;
    }
	
	// Update is called once per frame
	void Update () {
        if (ChatManager.isChatOpen)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            if(this.currentMapType == MapType.Mini)
            {
                ActivateMapMode(MapType.Large);
            }
            else if (this.currentMapType == MapType.Large)
            {
                ActivateMapMode(MapType.Mini);
            }
        }
    }

    public void ActivateMapMode(MapType map)
    {
        this.currentMapType = map;
        switch (map)
        {
            case MapType.Mini:
                itsMapSystem.SetTarget(PlayerManager.LocalPlayerInstance);
                itsMapSystem.EventClickedOnMinimap.Trigger(itsMapSystem, new KGFMapSystem.KGFClickEventArgs(PlayerManager.LocalPlayerInstance.transform.position));
                itsMapSystem.SetIconsVisibleByCategory("Blue_Cargo", true);
                itsMapSystem.SetIconsVisibleByCategory("Blue_Units", true);
                itsMapSystem.SetIconsVisibleByCategory("Red_Cargo", true);
                itsMapSystem.SetIconsVisibleByCategory("Red_Units", true);
                if (MasterHUD != null)
                {
                    MasterHUD.SetActive(true); 
                }

                if (MapModeHUD != null)
                {
                    MapModeHUD.SetActive(false);
                }

                if (SpawnModeHUD != null)
                {
                    SpawnModeHUD.SetActive(false);
                }

                this.itsMapSystem.SetFullscreen(false);
                Cursor.visible = false;
                break;
            case MapType.Large:
                itsMapSystem.SetTarget(MapCenter);
                itsMapSystem.EventClickedOnMinimap.Trigger(itsMapSystem, new KGFMapSystem.KGFClickEventArgs(MapCenter.transform.position));
                itsMapSystem.SetIconsVisibleByCategory("Blue_Cargo", true);
                itsMapSystem.SetIconsVisibleByCategory("Blue_Units", true);
                itsMapSystem.SetIconsVisibleByCategory("Red_Cargo", true);
                itsMapSystem.SetIconsVisibleByCategory("Red_Units", true);
                if (MasterHUD != null)
                {
                    MasterHUD.SetActive(false);
                }

                if (MapModeHUD != null)
                {
                    MapModeHUD.SetActive(true);
                }

                if (SpawnModeHUD != null)
                {
                    SpawnModeHUD.SetActive(false);
                }
                this.itsMapSystem.SetFullscreen(true);
                Cursor.visible = true;
                break;
            case MapType.Spawn:
                itsMapSystem.SetTarget(SpawnCenter);
                itsMapSystem.EventClickedOnMinimap.Trigger(itsMapSystem, new KGFMapSystem.KGFClickEventArgs(SpawnCenter.transform.position));
                itsMapSystem.SetIconsVisibleByCategory("Blue_Cargo", false);
                itsMapSystem.SetIconsVisibleByCategory("Blue_Units", false);
                itsMapSystem.SetIconsVisibleByCategory("Red_Cargo", false);
                itsMapSystem.SetIconsVisibleByCategory("Red_Units", false);
                if (MasterHUD != null)
                {
                    MasterHUD.SetActive(false);
                }

                if (MapModeHUD != null)
                {
                    MapModeHUD.SetActive(false);
                }

                if (SpawnModeHUD != null)
                {
                    SpawnModeHUD.SetActive(true);
                }

                this.itsMapSystem.SetFullscreen(true);
                Cursor.visible = true;
                break;
            default:
                break;
        }

        UpdateLockMode();
    }

    void OnMapMarkerClicked(object theSender, EventArgs theArgs)
	{
        //[ASSIGNED NEVER USED] PlayerManager player = PlayerManager.LocalPlayerInstance.GetComponent<PlayerManager>();
        if (this.currentMapType == MapType.Spawn)
        {
            KGFMapSystem.KGFMarkerEventArgs aMarkerArgs = (KGFMapSystem.KGFMarkerEventArgs)theArgs;
            KGFMapIcon mapIcon = (KGFMapIcon)aMarkerArgs.itsMarker;
            var foundObject = mapIcon.GetGameObject();
            var unitInfo = foundObject.GetComponent<Unit>();

            if (unitInfo.unitType == UnitType.RepairPad)
            {
                if (unitInfo.IsUnitFriendly())
                {
                    if (PlayerSpawnManager.status == SpawnStatus.IsReady)
                    {
                        PlayerSpawnManager.SpawnPlayer(foundObject.transform.position);
                    }
                }
            }
        }

	}

    private void UpdateLockMode()
    {
        if (Cursor.visible)
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}

public static class KGFMapSystemExts
{
    public static GameObject GetGameObject(this KGFMapIcon mapIcon)
    {
        return mapIcon.gameObject;
    }
}
