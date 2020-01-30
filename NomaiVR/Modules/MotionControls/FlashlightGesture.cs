﻿using UnityEngine;

namespace NomaiVR {
    class FlashlightGesture: MonoBehaviour {
        void Awake () {
            var detector = Common.PlayerHead.gameObject.AddComponent<ProximityDetector>();
            detector.other = Hands.RightHand;
            detector.localOffset = Vector3.right * 0.15f;
            detector.onEnter += FlashlightPress;
            detector.onExit += FlashlightRelease;
        }

        void FlashlightPress () {
            ControllerInput.SimulateButton(XboxButton.RightStickClick, 1);
        }
        void FlashlightRelease () {
            ControllerInput.SimulateButton(XboxButton.RightStickClick, 0);
        }
    }
}
