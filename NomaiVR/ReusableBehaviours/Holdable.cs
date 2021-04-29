﻿using OWML.Utils;
using System;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;

namespace NomaiVR
{
    public class Holdable : MonoBehaviour
    {
        public bool IsOffhand { get; set; } = false;
        public bool CanFlipX { get; set; } = true;
        public Action<bool> onFlipped;

        private Vector3 CurrentPositionOffset => PlayerHelper.IsWearingSuit() ? _glovePositionOffset : _handPositionOffset;
        private SteamVR_Skeleton_Pose CurrentHoldPose => PlayerHelper.IsWearingSuit() ? _gloveHoldPose : _handHoldPose;

        private Transform _hand = HandsController.Behaviour.DominantHand;
        private Transform _holdableTransform;
        private Transform _rotationTransform;
        private Quaternion _rotationOffset;
        private Vector3 _handPositionOffset;
        private Vector3 _glovePositionOffset;
        private SteamVR_Skeleton_Pose _handHoldPose = AssetLoader.GrabbingHandlePose;
        private SteamVR_Skeleton_Pose _gloveHoldPose = AssetLoader.GrabbingHandlePose;
        private SteamVR_Skeleton_Poser _poser;
        private IActiveObserver _activeObserver;

        public void SetPositionOffset(Vector3 handOffset, Vector3? gloveOffset = null)
        {
            _handPositionOffset = handOffset;
            _glovePositionOffset = gloveOffset ?? handOffset;
        }

        public void SetPoses(SteamVR_Skeleton_Pose handPose, SteamVR_Skeleton_Pose glovePose = null)
        {
            _handHoldPose = handPose;
            _gloveHoldPose = glovePose ?? handPose;
        }

        public void SetRotationOffset(Quaternion rotation)
        {
            _rotationOffset = rotation;
        }

        internal void Start()
        {
            _holdableTransform = new GameObject().transform;
            _holdableTransform.parent = _hand.GetComponent<Hand>().Palm;
            _holdableTransform.localPosition = CurrentPositionOffset;
            _holdableTransform.localRotation = Quaternion.identity;
            _rotationTransform = new GameObject().transform;
            _rotationTransform.SetParent(_holdableTransform, false);
            _rotationTransform.localPosition = Vector3.zero;
            _rotationTransform.localRotation = _rotationOffset;
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
            GlobalMessenger.AddListener("SuitUp", OnSuitChanged);
            GlobalMessenger.AddListener("RemoveSuit", OnSuitChanged);
        }

        internal void OnDestroy()
        {
            GlobalMessenger.RemoveListener("SuitUp", OnSuitChanged);
            GlobalMessenger.RemoveListener("RemoveSuit", OnSuitChanged);
            ModSettings.OnConfigChange -= OnInteractingHandChanged;
            VRToolSwapper.InteractingHandChanged -= OnInteractingHandChanged;
        }

        internal void OnSuitChanged()
        {
            _poser.skeletonMainPose = CurrentHoldPose;
            UpdateHoldableOffset(_hand == HandsController.Behaviour.RightHand);
        }

        private void SetupPoses()
        {
            transform.gameObject.SetActive(false);
            _poser = transform.gameObject.AddComponent<SteamVR_Skeleton_Poser>();
            _poser.skeletonMainPose = CurrentHoldPose;
            transform.gameObject.SetActive(true);

            //Listen for events to start poses
            Transform solveToolsTransform = transform.Find("Props_HEA_Signalscope") ??
                                            transform.Find("Props_HEA_ProbeLauncher") ??
                                            transform.Find("TranslatorGroup/Props_HEA_Translator"); //Tried to find the first renderer but the probelauncher has multiple of them, doing it this way for now...
            _activeObserver = transform.childCount > 0 ? (solveToolsTransform != null ? solveToolsTransform.gameObject.AddComponent<EnableObserver>() : null)
                                                                        : transform.gameObject.AddComponent<ChildThresholdObserver>() as IActiveObserver;

            // Both this holdable and the observer should be destroyed at the end of a cycle so no leaks here
            if (_activeObserver != null)
            {
                _activeObserver.OnActivate += () =>_hand.GetComponent<Hand>().NotifyAttachedTo(_poser);
                _activeObserver.OnDeactivate += () => _hand.GetComponent<Hand>().NotifyDetachedFrom(_poser);
            }
        }

        internal void UpdateHoldableOffset(bool isRight)
        {
            if (isRight)
                _holdableTransform.localPosition = CurrentPositionOffset;
            else
                _holdableTransform.localPosition = new Vector3(-CurrentPositionOffset.x, CurrentPositionOffset.y, CurrentPositionOffset.z);
        }

        internal void OnInteractingHandChanged()
        {
            if(VRToolSwapper.InteractingHand?.transform != _hand)
            {
                if (_hand != null && _activeObserver != null && _activeObserver.IsActive)
                    _hand.GetComponent<Hand>().NotifyDetachedFrom(_poser);

                _hand = IsOffhand ? VRToolSwapper.NonInteractingHand?.transform : VRToolSwapper.InteractingHand?.transform;
                if (_hand == null) _hand = IsOffhand ? HandsController.Behaviour.OffHand : HandsController.Behaviour.DominantHand;

                var handBehaviour = _hand.GetComponent<Hand>();
                _holdableTransform.SetParent(handBehaviour.Palm, false);

                var isRight = _hand == HandsController.Behaviour.RightHand;
                if (isRight)
                    _holdableTransform.localScale = new Vector3(1, 1, 1);
                else
                {
                    if (CanFlipX)
                        _holdableTransform.localScale = new Vector3(-1, 1, 1);
                }

                UpdateHoldableOffset(isRight);

                if (CanFlipX)
                {
                    RestoreCanvases(isRight);
                    onFlipped?.Invoke(isRight);
                }

                if (_hand != null && _activeObserver != null && _activeObserver.IsActive)
                    handBehaviour.NotifyAttachedTo(_poser);
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
