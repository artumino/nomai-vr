﻿using OWML.Utils;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace NomaiVR
{
    public class Holdable : MonoBehaviour
    {
        public bool IsOffhand { get; set; } = false;
        private Transform _hand = HandsController.Behaviour.DominantHand;
        public bool CanFlipX { get; set; } = true;
        public Action<bool> onFlipped;

        private Transform _holdableTransform;
        private Transform _rotationTransform;
        private Vector3 _positionOffset;

        internal void Start()
        {
            _holdableTransform = new GameObject().transform;
            _holdableTransform.localPosition = _positionOffset = transform.localPosition;
            _holdableTransform.localRotation = Quaternion.identity;
            _rotationTransform = new GameObject().transform;
            _rotationTransform.SetParent(_holdableTransform, false);
            _rotationTransform.localPosition = Vector3.zero;
            _rotationTransform.localRotation = transform.localRotation;
            transform.parent = _rotationTransform;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            OnInteractingHandChanged();
            var tool = gameObject.GetComponent<PlayerTool>();
            if (tool)
            {
                tool._stowTransform = null;
                tool._holdTransform = null;
            }

            VRToolSwapper.InteractingHandChanged += OnInteractingHandChanged;
            ModSettings.OnConfigChange += OnInteractingHandChanged;
        }

        internal void OnDestroy()
        {
            ModSettings.OnConfigChange -= OnInteractingHandChanged;
            VRToolSwapper.InteractingHandChanged -= OnInteractingHandChanged;
        }
        
        internal void UpdateHand()
        {
            _hand = IsOffhand ? VRToolSwapper.NonInteractingHand?.transform : VRToolSwapper.InteractingHand?.transform;
            if (_hand == null) _hand = IsOffhand ? HandsController.Behaviour.OffHand : HandsController.Behaviour.DominantHand;
        }

        internal void OnInteractingHandChanged()
        {
            if(VRToolSwapper.InteractingHand?.transform != _hand)
            {
                UpdateHand();
                _holdableTransform.SetParent(_hand, false);

                var isRight = _hand == HandsController.Behaviour.RightHand;
                if (isRight)
                {
                    _holdableTransform.localScale = new Vector3(1, 1, 1);
                    _holdableTransform.localPosition = _positionOffset;
                }
                else
                {
                    if (CanFlipX)
                        _holdableTransform.localScale = new Vector3(-1, 1, 1);
                    _holdableTransform.localPosition = new Vector3(-_positionOffset.x, _positionOffset.y, _positionOffset.z);
                }

                if (CanFlipX)
                {
                    RestoreCanvases(isRight);
                    onFlipped?.Invoke(isRight);
                }
            }
        }

        internal void RestoreCanvases(bool isRight)
        {
            //Assures canvases are always scaled with x > 0
            Array.ForEach(transform.GetComponentsInChildren<Canvas>(true), canvas =>
            {
                Transform canvasTransform = canvas.transform;
                Vector3 canvasScale = canvasTransform.localScale;
                float tagetScale = Mathf.Abs(canvasScale.x);
                if (!isRight) tagetScale *= -1;
                canvasTransform.localScale = new Vector3(tagetScale, canvasScale.y, canvasScale.z);
            });
        }
    }
}
