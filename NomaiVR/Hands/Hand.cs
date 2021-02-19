﻿using UnityEngine;
using Valve.VR;
using Valve.VR.Helpers;

namespace NomaiVR
{
    public class Hand : MonoBehaviour
    {
        public GameObject handPrefab;
        public GameObject glovePrefab;
        public SteamVR_Action_Pose pose;
        public SteamVR_Action_Single fallbackCurl;
        public bool isLeft;

        internal void Start()
        {
            SetUpHandModel();
            SetUpGloveModel();
            SetUpVrPose();
        }

        private void SetUpHandModel()
        {
            var handObject = Instantiate(handPrefab);
            var hand = handObject.transform;
            hand.gameObject.AddComponent<ConditionalRenderer>().getShouldRender += ShouldRenderHands;
            hand.GetComponentInChildren<Renderer>().material.shader = Shader.Find("Outer Wilds/Character/Skin");
            SetUpModel(hand);
            SetUpSkeletons(handObject, hand);
        }

        private void SetUpGloveModel()
        {
            var glove = Instantiate(glovePrefab).transform;
            glove.gameObject.AddComponent<ConditionalRenderer>().getShouldRender += ShouldRenderGloves;
            glove.GetComponentInChildren<Renderer>().material.shader = Shader.Find("Outer Wilds/Character/Clothes");
            SetUpModel(glove);
        }

        private void SetUpModel(Transform model)
        {
            model.parent = transform;
            model.localPosition = transform.localPosition;
            model.localRotation = transform.localRotation;
            model.localScale = Vector3.one * 6;
            if (isLeft)
            {
                model.localScale = new Vector3(-model.localScale.x, model.localScale.y, model.localScale.z);
            }
        }

        private static string BoneToTarget(string bone, bool isSource) => isSource ? $"SourceSkeleton/Root/{bone}" : $"vr_alien_hand/Root/{bone}";
        
        private static string FingerBoneName(string fingerName, int depth)
        {
            string name = $"finger_{fingerName}_meta_r";
            for (int i = 0; i < depth; i++)
                name += $"/finger_{fingerName}_{i}_r";
            return name;
        }

        private static string ThumbBoneName(string fingerName, int depth)
        {
            string name = $"finger_{fingerName}_0_r";
            for (int i = 0; i < depth; i++)
                name += $"/finger_{fingerName}_{i + 1}_r";
            return name;
        }

        private void SetUpSkeletons(GameObject gameObject, Transform transform)
        {
            var skeletonDriver = gameObject.AddComponent<SteamVR_Behaviour_Skeleton>();
            skeletonDriver.inputSource = SteamVR_Input_Sources.LeftHand;
            skeletonDriver.rangeOfMotion = EVRSkeletalMotionRange.WithoutController;
            skeletonDriver.skeletonRoot = transform.Find("SourceSkeleton/Root");
            skeletonDriver.updatePose = false;
            skeletonDriver.skeletonBlend = 1;

            if(isLeft)
                skeletonDriver.mirroring = SteamVR_Behaviour_Skeleton.MirrorType.RightToLeft;

            //skeletonDriver.skeletonAction = SteamVR_Input.GetAction<SteamVR_Action_Skeleton>("Skeleton" + skeletonDriver.inputSource.ToString());
            skeletonDriver.fallbackCurlAction = fallbackCurl;
            skeletonDriver.enabled = true;

            var skeletonRetargeter = transform.gameObject.AddComponent<CustomSkeletonHelper>();
            var sourceWristTransform = transform.Find(BoneToTarget("wrist_r", true));
            var targetWristTransform = transform.Find(BoneToTarget("wrist_r", false));
            skeletonRetargeter.wrist = new CustomSkeletonHelper.Retargetable(sourceWristTransform, targetWristTransform);
            skeletonRetargeter.thumbs = new CustomSkeletonHelper.Thumb[1] {
                new CustomSkeletonHelper.Thumb(
                    new CustomSkeletonHelper.Retargetable(sourceWristTransform.Find(ThumbBoneName("thumb", 0)), targetWristTransform.Find(ThumbBoneName("thumb", 0))), //Metacarpal
                    new CustomSkeletonHelper.Retargetable(sourceWristTransform.Find(ThumbBoneName("thumb", 1)), targetWristTransform.Find(ThumbBoneName("thumb", 1))), //Middle
                    new CustomSkeletonHelper.Retargetable(sourceWristTransform.Find(ThumbBoneName("thumb", 2)), targetWristTransform.Find(ThumbBoneName("thumb", 2))), //Distal
                    transform.Find(BoneToTarget("finger_thumb_r_aux", true)) //aux
                )
            };
            skeletonRetargeter.fingers = new CustomSkeletonHelper.Finger[2]
            {
                new CustomSkeletonHelper.Finger(
                    new CustomSkeletonHelper.Retargetable(sourceWristTransform.Find(FingerBoneName("index", 0)), targetWristTransform.Find(FingerBoneName("index", 0))), //Metacarpal
                    new CustomSkeletonHelper.Retargetable(sourceWristTransform.Find(FingerBoneName("index", 1)), targetWristTransform.Find(FingerBoneName("index", 1))), //Proximal
                    new CustomSkeletonHelper.Retargetable(sourceWristTransform.Find(FingerBoneName("index", 2)), targetWristTransform.Find(FingerBoneName("index", 2))), //Middle
                    new CustomSkeletonHelper.Retargetable(sourceWristTransform.Find(FingerBoneName("index", 3)), targetWristTransform.Find(FingerBoneName("index", 3))), //Distal
                    transform.Find(BoneToTarget("finger_index_r_aux", true)) //aux
                ),
                new CustomSkeletonHelper.Finger(
                    new CustomSkeletonHelper.Retargetable(sourceWristTransform.Find(FingerBoneName("ring", 0)), targetWristTransform.Find(FingerBoneName("ring", 0))), //Metacarpal
                    new CustomSkeletonHelper.Retargetable(sourceWristTransform.Find(FingerBoneName("ring", 1)), targetWristTransform.Find(FingerBoneName("ring", 1))), //Proximal
                    new CustomSkeletonHelper.Retargetable(sourceWristTransform.Find(FingerBoneName("ring", 2)), targetWristTransform.Find(FingerBoneName("ring", 2))), //Middle
                    new CustomSkeletonHelper.Retargetable(sourceWristTransform.Find(FingerBoneName("ring", 3)), targetWristTransform.Find(FingerBoneName("ring", 3))), //Distal
                    transform.Find(BoneToTarget("finger_ring_r_aux", true)) //aux
                ),
            };
        }

        private void SetUpVrPose()
        {
            gameObject.SetActive(false);

            var poseDriver = transform.gameObject.AddComponent<SteamVR_Behaviour_Pose>();
            poseDriver.poseAction = pose;

            gameObject.SetActive(true);
        }

        private bool ShouldRenderGloves()
        {
            return SceneHelper.IsInGame() && PlayerHelper.IsWearingSuit();
        }

        private bool ShouldRenderHands()
        {
            return !SceneHelper.IsInGame() || !PlayerHelper.IsWearingSuit();
        }
    }
}
