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

        public SteamVR_Skeleton_Pose holdPose = AssetLoader.GrabbingHandlePose;
        private SteamVR_Skeleton_Poser _poser;

        internal void Start()
        {
            _holdableTransform = new GameObject().transform;
            _holdableTransform.parent = _hand;
            _holdableTransform.localPosition = _positionOffset = transform.localPosition;
            _holdableTransform.localRotation = Quaternion.identity;
            _rotationTransform = new GameObject().transform;
            _rotationTransform.SetParent(_holdableTransform, false);
            _rotationTransform.localPosition = Vector3.zero;
            _rotationTransform.localRotation = transform.localRotation;
            transform.parent = _rotationTransform;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            var tool = gameObject.GetComponent<PlayerTool>();
            if (tool)
            {
                tool._stowTransform = null;
                tool._holdTransform = null;
            }

            SetupPoses();

            VRToolSwapper.InteractingHandChanged += OnInteractingHandChanged;
            ModSettings.OnConfigChange += OnInteractingHandChanged;
        }

        internal void OnDestroy()
        {
            ModSettings.OnConfigChange -= OnInteractingHandChanged;
            VRToolSwapper.InteractingHandChanged -= OnInteractingHandChanged;
        }

        private void SetupPoses()
        {
            transform.gameObject.SetActive(false);
            _poser = transform.gameObject.AddComponent<SteamVR_Skeleton_Poser>();
            _poser.skeletonMainPose = holdPose;
            transform.gameObject.SetActive(true);

            //Listen for events to start poses
            Transform solveToolsTransform = transform.Find("Props_HEA_Signalscope") ??
                                            transform.Find("Props_HEA_ProbeLauncher") ??
                                            transform.Find("TranslatorGroup/Props_HEA_Translator"); //Tried to find the first renderer bu the probelauncher has multiple of them, doing it this way for now...
            IActiveObserver enableObserver = transform.childCount > 0 ? (solveToolsTransform != null ? solveToolsTransform.gameObject.AddComponent<EnableObserver>() : null)
                                                                        : transform.gameObject.AddComponent<ChildThresholdObserver>() as IActiveObserver;

            // Both this holdable and the observer should be destroyed at the end of a cycle so no leaks here
            if (enableObserver != null)
            {
                enableObserver.OnActivate += () => hand.NotifyAttachedTo(_poser);
                enableObserver.OnDeactivate += () => hand.NotifyDetachedFrom(_poser);
            }
        }

        internal void OnInteractingHandChanged()
        {
            if(VRToolSwapper.InteractingHand?.transform != _hand)
            {
                _hand = IsOffhand ? VRToolSwapper.NonInteractingHand?.transform : VRToolSwapper.InteractingHand?.transform;
                if (_hand == null) _hand = IsOffhand ? HandsController.Behaviour.OffHand : HandsController.Behaviour.DominantHand;
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
