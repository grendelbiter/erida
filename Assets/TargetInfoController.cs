using Assets.Wulfram3.Scripts.InternalApis.Classes;
using Greyman;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Wulfram3 {
    public class TargetInfoController : MonoBehaviour {


        public GameObject targetInfoPanel;
        public Text hitpoints;
        public Text myName;
        public Text team;
        public Text user;

        [HideInInspector]
        public GameObject target;
        private Vector3 pos;

        private OffScreenIndicator offScreenIndicator; //0 red, 1 blue
        // Use this for initialization
        void Start() {
            offScreenIndicator = FindObjectOfType<OffScreenIndicator>();
            foreach (var item in offScreenIndicator.targetList)
            {
                offScreenIndicator.RemoveIndicator(item.target);
            }
            FindObjectOfType<GameManager>().AddTargetChangeListener(this);
            TargetChanged(null);
        }

        // Update is called once per frame
        void LateUpdate() {
            //if (target != null && target.GetComponentInChildren<MeshRenderer>().isVisible && Camera.main != null)


            if (target != null  && Camera.main != null) {
                targetInfoPanel.SetActive(true);
                pos = Camera.main.WorldToScreenPoint(target.transform.position);
                pos.z = 0;
                RectTransform rectTransform = GetComponent<RectTransform>();
                pos.y -= 20;
                //rectTransform.localPosition = new Vector2(0, 100);
                rectTransform.SetPositionAndRotation(pos, rectTransform.rotation);

                PlayerManager player = PlayerManager.LocalPlayerInstance.GetComponent<PlayerManager>();
                float dist = (float) Math.Round(Vector3.Distance(target.transform.position, player.transform.position), 0);


                Unit unit = target.GetComponent<Unit>();

                if (unit == null)
                {
                    Logger.Exception(new Exception("GAME OBJECT IS MISSING UNIT"));
                    TargetChanged(null);
                    return;
                }
                // Check if targetd unit is now dead, if so, clear
                if(unit.isDead)
                {
                    TargetChanged(null);
                    return;
                }

                hitpoints.text = dist + "M " + unit.health + "HP";
                myName.text = unit.unitType.ToString();
                team.text = unit.unitTeam.ToString();

                //[ASSIGNED NEVER USED] var panel = targetInfoPanel.GetComponent<Image>();
                switch (unit.unitTeam)
                {
                    case PunTeams.Team.none:
                        myName.color = FindObjectOfType<GameManager>().graycolor.color;
                        break;
                    case PunTeams.Team.Red:
                        myName.color = FindObjectOfType<GameManager>().redcolor.color;
                        break;
                    case PunTeams.Team.Blue:
                        myName.color = FindObjectOfType<GameManager>().bluecolor.color;
                        break;
                    default:
                        myName.color = FindObjectOfType<GameManager>().graycolor.color;
                        break;
                }

                if(unit.unitType == UnitType.Tank || unit.unitType == UnitType.Scout)
                {
                    user.text = FindObjectOfType<GameManager>().GetColoredPlayerName(target.GetComponent<PhotonView>().owner.NickName, target.GetComponent<PhotonView>().owner.IsMasterClient);
                }
                else
                {
                    user.text = "";
                }
                
            }
            else {
                targetInfoPanel.SetActive(false);
            }
        }

        public void TargetChanged(GameObject t) {
            target = t;

            if (t == null) {
                targetInfoPanel.SetActive(false);
                var temp = offScreenIndicator.targetList.ToList();
                foreach (var item in temp)
                {
                    offScreenIndicator.RemoveIndicator(item.target);
                }
            }
            else {
                var temp = offScreenIndicator.targetList.ToList();
                foreach (var item in temp)
                {
                    offScreenIndicator.RemoveIndicator(item.target);
                }

                switch (target.GetComponent<Unit>().unitTeam)
                {
                    case PunTeams.Team.none:
                        offScreenIndicator.AddIndicator(target.transform, 2);
                        break;
                    case PunTeams.Team.Red:
                        offScreenIndicator.AddIndicator(target.transform, 0);
                        break;
                    case PunTeams.Team.Blue:
                        offScreenIndicator.AddIndicator(target.transform, 1);
                        break;
                    default:
                        offScreenIndicator.AddIndicator(target.transform, 2);
                        break;
                }
                targetInfoPanel.SetActive(true);
            }
        }

        public void OnDrawGizmos() {
            if (target != null) {
                //drawString("blah", target.transform.position, Color.blue);
            }
        }

        //static public void drawString(string text, Vector3 worldPos, Color? colour = null) {
        //    UnityEditor.Handles.BeginGUI();

        //    var restoreColor = GUI.color;

        //    if (colour.HasValue) GUI.color = colour.Value;
        //    Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

        //    if (screenPos.y < 0 || screenPos.y > Screen.height || screenPos.x < 0 || screenPos.x > Screen.width || screenPos.z < 0) {
        //        GUI.color = restoreColor;
        //        UnityEditor.Handles.EndGUI();
        //        return;
        //    }

        //    Vector2 size = GUI.skin.label.CalcSize(new GUIContent(text));
        //    GUI.Label(new Rect(screenPos.x, Screen.height - screenPos.y, size.x, size.y), text);
        //    GUI.color = restoreColor;
        //    UnityEditor.Handles.EndGUI();
        //}

        //static public void drawString2(string text, Vector3 worldPos, Color? colour = null) {
        //    UnityEditor.Handles.BeginGUI();

        //    var restoreColor = GUI.color;

        //    if (colour.HasValue) GUI.color = colour.Value;
        //    var view = UnityEditor.SceneView.currentDrawingSceneView;
        //    Vector3 screenPos = view.camera.WorldToScreenPoint(worldPos);

        //    if (screenPos.y < 0 || screenPos.y > Screen.height || screenPos.x < 0 || screenPos.x > Screen.width || screenPos.z < 0) {
        //        GUI.color = restoreColor;
        //        UnityEditor.Handles.EndGUI();
        //        return;
        //    }

        //    Vector2 size = GUI.skin.label.CalcSize(new GUIContent(text));
        //    GUI.Label(new Rect(screenPos.x - (size.x / 2), -screenPos.y + view.position.height + 4, size.x, size.y), text);
        //    GUI.color = restoreColor;
        //    UnityEditor.Handles.EndGUI();
        //}
    }
}
