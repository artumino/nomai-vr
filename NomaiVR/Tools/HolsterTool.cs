﻿using OWML.ModHelper.Events;
using System;
using UnityEngine;

namespace NomaiVR {
    class HolsterTool: MonoBehaviour {
        public Transform hand;
        public ToolMode mode;
        public Vector3 position;
        public Vector3 angle;
        public float scale;
        MeshRenderer[] _renderers;
        bool _visible;
        bool _enabled = true;
        Grabbable _grabbable;
        public Action onEquip;
        public Action onUnequip;

        void Start () {
            _renderers = gameObject.GetComponentsInChildren<MeshRenderer>();
            _grabbable = gameObject.AddComponent<Grabbable>();
            _grabbable.onGrab += OnGrab;
            _grabbable.onRelease += OnRelease;
            _grabbable.detector.minDistance = 0.2f;
            transform.localScale = Vector3.one * scale;
        }

        void OnGrab () {
            Equip();
        }

        void OnRelease () {
            Unequip();
        }

        void Equip () {
            onEquip?.Invoke();
            Locator.GetToolModeSwapper().EquipToolMode(mode);

            if (mode == ToolMode.Translator) {
                GameObject.FindObjectOfType<NomaiTranslatorProp>().SetValue("_currentTextID", 1);
            }
        }

        void Unequip () {
            onUnequip?.Invoke();
            Common.ToolSwapper.UnequipTool();
        }

        void SetVisible (bool visible) {
            foreach (var renderer in _renderers) {
                renderer.enabled = visible;
            }
            _visible = visible;
        }

        void Update () {
            if (_enabled && !OWInput.IsInputMode(InputMode.Character)) {
                _enabled = false;
                SetVisible(false);
            }
            if (!_enabled && OWInput.IsInputMode(InputMode.Character)) {
                _enabled = true;
            }
            if (!_enabled) {
                return;
            }
            if (!_visible && !Common.ToolSwapper.IsInToolMode(mode)) {
                SetVisible(true);
            }
            if (_visible && Common.ToolSwapper.IsInToolMode(mode)) {
                SetVisible(false);
            }
        }

        void LateUpdate () {
            if (!_enabled) {
                return;
            }
            if (_enabled && _visible) {
                transform.position = Camera.main.transform.position + Common.PlayerBody.transform.TransformVector(position);
                transform.rotation = Common.PlayerBody.transform.rotation;
                transform.Rotate(angle);
            }
        }
    }
}