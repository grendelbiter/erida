using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;// Required when using Event data.

namespace Com.Wulfram3 {
	public class CursorVisibilityManager : MonoBehaviour {

        // Use this for initialization
        void Start() {
            Cursor.visible = false;
            UpdateLockMode();
        }

        // Update is called once per frame
        void Update() {
            if (Input.GetKeyDown(KeyCode.Escape)) {
                Cursor.visible = !Cursor.visible;
                UpdateLockMode();
                Logger.Log("ESC was pressed");
            }
			if (Input.GetMouseButtonDown(0)){
				UpdateLockMode();
                Logger.Log("Cursor locked!");
			}
		
        }

        private void UpdateLockMode() {
            if (Cursor.visible) {
                Cursor.lockState = CursorLockMode.None;
            } else {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }
}
