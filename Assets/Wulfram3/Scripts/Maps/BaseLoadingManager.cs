﻿using Assets.Wulfram3.Scripts.InternalApis.Classes;
using Com.Wulfram3;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Assets.Wulfram3.Scripts.Maps
{
    public class BaseLoadingManager : MonoBehaviour
    {

        // Use this for initialization
        void Start()
        {
            MapSetup("temp");
        }

        // Update is called once per frame
        void Update()
        {

        }

        [PunRPC]
        void MapSetup(string mapName)
        {
            if (PhotonNetwork.isMasterClient)
            {
                var data = @"{
    ""mapName"": ""Rancor"",
    ""loadNumber"": 999,
    ""bases"": [{
        ""baseName"": ""base1"",
        ""team"": 1,
        ""units"": [{
            ""unitType"": 3,
            ""posX"": 317,
            ""posY"": 17,
            ""posZ"": 692,
            ""cargoType"": 5
        }, {
            ""unitType"": 3,
            ""posX"": 346,
            ""posY"": 27,
            ""posZ"": 675,
            ""cargoType"": 4
        }, {
            ""unitType"": 9,
            ""posX"": 324,
            ""posY"": 26,
            ""posZ"": 675
        }, {
            ""unitType"": 11,
            ""posX"": 307,
            ""posY"": 19,
            ""posZ"": 687
        }, {
            ""unitType"": 12,
            ""posX"": 307,
            ""posY"": 17,
            ""posZ"": 682
        }, {
            ""unitType"": 3,
            ""posX"": 348,
            ""posY"": 28,
            ""posZ"": 676,
            ""cargoType"": 8
        }, {
            ""unitType"": 3,
            ""posX"": 319,
            ""posY"": 18,
            ""posZ"": 692,
            ""cargoType"": 7
        }, {
            ""unitType"": 5,
            ""posX"": 301,
            ""posY"": 20,
            ""posZ"": 679
        }, {
            ""unitType"": 8,
            ""posX"": 336,
            ""posY"": 32,
            ""posZ"": 655
        }, {
            ""unitType"": 4,
            ""posX"": 307,
            ""posY"": 20,
            ""posZ"": 680
        }, {
            ""unitType"": 7,
            ""posX"": 312,
            ""posY"": 43,
            ""posZ"": 648
        }, {
            ""unitType"": 8,
            ""posX"": 291,
            ""posY"": 32,
            ""posZ"": 660
        }, {
            ""unitType"": 3,
            ""posX"": 356,
            ""posY"": 46,
            ""posZ"": 666,
            ""cargoType"": 8
        }, {
            ""unitType"": 3,
            ""posX"": 319,
            ""posY"": 17,
            ""posZ"": 693,
            ""cargoType"": 8
        }, {
            ""unitType"": 3,
            ""posX"": 322,
            ""posY"": 17,
            ""posZ"": 692,
            ""cargoType"": 7
        }, {
            ""unitType"": 3,
            ""posX"": 337,
            ""posY"": 28,
            ""posZ"": 674,
            ""cargoType"": 4
        }, {
            ""unitType"": 10,
            ""posX"": 332,
            ""posY"": 18,
            ""posZ"": 689
        }, {
            ""unitType"": 7,
            ""posX"": 296,
            ""posY"": 29,
            ""posZ"": 691
        }, {
            ""unitType"": 4,
            ""posX"": 305,
            ""posY"": 21,
            ""posZ"": 682
        }]
    }, {
        ""baseName"": ""base1"",
        ""team"": 2,
        ""units"": [{
            ""unitType"": 4,
            ""posX"": 69,
            ""posY"": 22,
            ""posZ"": 35
        }, {
            ""unitType"": 3,
            ""posX"": 66,
            ""posY"": 21,
            ""posZ"": 17,
            ""cargoType"": 4
        }, {
            ""unitType"": 7,
            ""posX"": 78,
            ""posY"": 46,
            ""posZ"": 42
        }, {
            ""unitType"": 11,
            ""posX"": 68,
            ""posY"": 22,
            ""posZ"": 38
        }, {
            ""unitType"": 3,
            ""posX"": 64,
            ""posY"": 21,
            ""posZ"": 14,
            ""cargoType"": 5
        }, {
            ""unitType"": 3,
            ""posX"": 73,
            ""posY"": 20,
            ""posZ"": 15,
            ""cargoType"": 4
        }, {
            ""unitType"": 12,
            ""posX"": 66,
            ""posY"": 21,
            ""posZ"": 24
        }, {
            ""unitType"": 3,
            ""posX"": 74,
            ""posY"": 20,
            ""posZ"": 18,
            ""cargoType"": 8
        }, {
            ""unitType"": 3,
            ""posX"": 75,
            ""posY"": 20,
            ""posZ"": 16,
            ""cargoType"": 7
        }, {
            ""unitType"": 8,
            ""posX"": 85,
            ""posY"": 25,
            ""posZ"": 16
        }, {
            ""unitType"": 4,
            ""posX"": 68,
            ""posY"": 22,
            ""posZ"": 35
        }, {
            ""unitType"": 3,
            ""posX"": 68,
            ""posY"": 21,
            ""posZ"": 15,
            ""cargoType"": 8
        }, {
            ""unitType"": 5,
            ""posX"": 66,
            ""posY"": 22,
            ""posZ"": 33
        }, {
            ""unitType"": 8,
            ""posX"": 56,
            ""posY"": 27,
            ""posZ"": 49
        }, {
            ""unitType"": 3,
            ""posX"": 71,
            ""posY"": 20,
            ""posZ"": 17,
            ""cargoType"": 7
        }, {
            ""unitType"": 9,
            ""posX"": 70,
            ""posY"": 21,
            ""posZ"": 27
        }, {
            ""unitType"": 7,
            ""posX"": 54,
            ""posY"": 29,
            ""posZ"": 16
        }, {
            ""unitType"": 10,
            ""posX"": 90,
            ""posY"": 24,
            ""posZ"": 38
        }, {
            ""unitType"": 3,
            ""posX"": 63,
            ""posY"": 21,
            ""posZ"": 18,
            ""cargoType"": 4
        }]
    }]
}";
                var baseSetup = Newtonsoft.Json.JsonConvert.DeserializeObject<BaseSetup>(data);
                foreach (var item in baseSetup.bases)
                {
                    CreateBase(item);
                }
            }
        }

        private void CreateBase(Base singleBase)
        {
            foreach (var unit in singleBase.units)
            {
                var obj = PhotonNetwork.InstantiateSceneObject(Unit.GetPrefabName(unit.unitType,singleBase.team), new Vector3(unit.posX, unit.posY, unit.posZ), Quaternion.identity,0, null);
                if(unit.unitType == UnitType.Cargo)
                {
                    obj.GetComponent<Cargo>().team = singleBase.team;
                    obj.GetComponent<Cargo>().content = unit.cargoType;
                }
            }
        }

        public BaseSetup GetCurrentMapConfig()
        {
            //var tempTargets = new List<UnitTarget>(); // Commented to supress warning message (2/19/2018 : Cheebsta)  
            // Get Units on the map as it loads
            var units = ((Unit[])GameObject.FindObjectsOfType(typeof(Unit))).ToList();
            if (units.Count > 0)
            {
                var baseSetup = new BaseSetup();
                baseSetup.loadNumber = 999;
                baseSetup.mapName = "";
                baseSetup.bases = new List<Base>();
                foreach (var teamUnits in units.GroupBy(u => u.unitTeam))
                {
                    var tempBase = new Base();
                    tempBase.team = teamUnits.Key;
                    tempBase.baseName = "base1";
                    tempBase.units = new List<BaseUnit>();
                    foreach (var unit in teamUnits)
                    {
                        tempBase.units.Add(
                            new BaseUnit
                            {
                                unitType = unit.unitType,
                                posX = (int)unit.gameObject.transform.position.x,
                                posY = (int)unit.gameObject.transform.position.y,
                                posZ = (int)unit.gameObject.transform.position.z,
                                cargoType = unit.unitType == UnitType.Cargo ? unit.GetComponentInParent<Cargo>().content : UnitType.None
                            });
                    }

                    baseSetup.bases.Add(tempBase);
                }

                Logger.Log(Newtonsoft.Json.JsonConvert.SerializeObject(baseSetup));
                return baseSetup;
            }
            else
            {
                return null;

            }
        }
    }
}