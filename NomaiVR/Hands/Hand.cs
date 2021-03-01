using System.Linq;
using UnityEngine;
using Valve.VR;
using Valve.VR.Helpers;

namespace NomaiVR
{
    public class Hand : MonoBehaviour
    {
        public GameObject handPrefab;
        public SteamVR_Action_Pose pose;
        public SteamVR_Skeleton_Pose fallbackPoint;
        public SteamVR_Skeleton_Pose fallbackRelax;
        public SteamVR_Skeleton_Pose fallbackFist;
        public bool isLeft;

        public Transform Palm { get; private set; }

        private SkinnedMeshRenderer _renderer;
        private Material[] _handMaterials;
        private Material[] _gloveMaterials;
        private Mesh _gloveMesh;
        private Mesh _handMesh;
        private EHandModel _currentModel;
        private SteamVR_Behaviour_Skeleton _skeleton;
        private EVRSkeletalMotionRange _rangeOfMotion = EVRSkeletalMotionRange.WithoutController;

        private enum EHandModel
        {
            Disabled,
            Hand,
            Glove
        }

        internal void Start()
        {
            SetUpModel();
            SetUpVrPose();
        }

        private void SetGloves(bool hasGloves)
        {
            if(hasGloves)
            {
                _renderer.materials = _gloveMaterials;
                _renderer.sharedMesh = _gloveMesh;
            }
            else
            {
                _renderer.materials = _handMaterials;
                _renderer.sharedMesh = _handMesh;
            }
        }

        private void SetUpModel()
        {
            var handObject = Instantiate(handPrefab);
            handObject.SetActive(false);
            var hand = handObject.transform;

            _renderer = handObject.GetComponentInChildren<SkinnedMeshRenderer>(true);

            //We setup elements for hand/glove swapping
            _handMaterials = _renderer.materials;
            _handMesh = _renderer.sharedMesh;
            SetUpShaders(_handMaterials, "Outer Wilds/Character/Clothes", "Outer Wilds/Character/Skin");

            //Since our glove is only a prefab we have to instantiate materials that are editable, we clone them to a new array
            var gloveRenderer = AssetLoader.GlovePrefab.GetComponentInChildren<SkinnedMeshRenderer>(true);
            _gloveMaterials = gloveRenderer.materials.Select(x => new Material(x)).ToArray();
            SetUpShaders(_gloveMaterials, "Outer Wilds/Character/Clothes");
            _gloveMesh = gloveRenderer.sharedMesh;

            SetUpModel(hand);
            _skeleton = SetUpSkeleton(handObject, hand);
            Palm = handObject.transform.Find("Armature/Root/wrist_r");

            handObject.SetActive(true);
        }

        private void SetUpShaders(Material[] materials, params string[] shader)
        {
            if (shader.Length == 0)
                return;

            Shader[] toAssign = shader.Select(x => Shader.Find(x)).ToArray();
            for (int i = 0; i < materials.Length; i++)
                materials[i].shader = toAssign[Mathf.Clamp(i, 0, toAssign.Length)];
        }

        private void SetUpModel(Transform model)
        {
            model.parent = transform;
            model.localPosition = Vector3.zero;
            model.localRotation = Quaternion.identity;
            model.localScale = Vector3.one;
        }

        internal void NotifyReachable(bool canReach)
        {
            if (canReach)
                _skeleton.BlendToAnimation();
            else
                _skeleton.BlendToSkeleton();
        }

        internal void NotifyAttachedTo(SteamVR_Skeleton_Poser poser)
        {
            _skeleton.BlendToPoser(poser);
        }

        internal void NotifyDetachedFrom(SteamVR_Skeleton_Poser poser)
        {
            _skeleton.BlendToSkeleton();
        }

        private SteamVR_Behaviour_Skeleton SetUpSkeleton(GameObject prefabObject, Transform prefabTransform)
        {
            var skeletonDriver = prefabObject.AddComponent<SteamVR_Behaviour_Skeleton>();
            skeletonDriver.inputSource = isLeft ? SteamVR_Input_Sources.LeftHand : SteamVR_Input_Sources.RightHand;
            skeletonDriver.rangeOfMotion = _rangeOfMotion;
            skeletonDriver.skeletonRoot = prefabTransform.Find("Armature/Root");
            skeletonDriver.updatePose = false;
            //skeletonDriver.onlySetRotations = true;
            skeletonDriver.skeletonBlend = 1f;
            skeletonDriver.fallbackPoser = prefabObject.AddComponent<SteamVR_Skeleton_Poser>();

            if (isLeft)
            {
                //Flip X axis of skeleton and skinned meshes
                prefabTransform.localScale = new Vector3(-1, 1, 1);

                //Enable SteamVR skeleton mirroring
                skeletonDriver.mirroring = SteamVR_Behaviour_Skeleton.MirrorType.RightToLeft;
            }
            skeletonDriver.skeletonAction = isLeft ? SteamVR_Actions.default_SkeletonLeftHand : SteamVR_Actions.default_SkeletonRightHand;

            skeletonDriver.fallbackCurlAction = SteamVR_Actions.default_Squeeze;
            skeletonDriver.enabled = true;

            var skeletonPoser = skeletonDriver.fallbackPoser;
            skeletonPoser.skeletonMainPose = fallbackRelax;
            skeletonPoser.skeletonAdditionalPoses.Add(fallbackPoint);
            skeletonPoser.skeletonAdditionalPoses.Add(fallbackFist);

            //Point Fallback
            skeletonPoser.blendingBehaviours.Add(new SteamVR_Skeleton_Poser.PoseBlendingBehaviour()
            {
                action_bool = SteamVR_Actions.default_Grip,
                enabled = true,
                influence = 1,
                name = "point",
                pose = 1,
                value = 0,
                type = SteamVR_Skeleton_Poser.PoseBlendingBehaviour.BlenderTypes.BooleanAction,
                previewEnabled = true,
                smoothingSpeed = 16
            });

            //Fist Fallback
            skeletonPoser.blendingBehaviours.Add(new SteamVR_Skeleton_Poser.PoseBlendingBehaviour()
            {
                action_bool = SteamVR_Actions.default_GrabPinch,
                enabled = true,
                influence = 1,
                name = "fist",
                pose = 2,
                value = 0,
                type = SteamVR_Skeleton_Poser.PoseBlendingBehaviour.BlenderTypes.BooleanAction,
                previewEnabled = true,
                smoothingSpeed = 16
            });

            return skeletonDriver;
        }

        private void SetUpVrPose()
        {
            gameObject.SetActive(false);

            var poseDriver = transform.gameObject.AddComponent<SteamVR_Behaviour_Pose>();
            poseDriver.poseAction = pose;

            gameObject.SetActive(true);
        }

        public void SetLimitRangeOfMotion(bool isShown)
        {
            _rangeOfMotion = isShown ? EVRSkeletalMotionRange.WithController : EVRSkeletalMotionRange.WithoutController;
            _skeleton?.SetRangeOfMotion(_rangeOfMotion); // Back to main menu we have a nullreference here
        }

        internal void Update()
        {
            bool shouldRenderHands = ShouldRenderHands();
            bool shouldRenderGloves = ShouldRenderGloves();
            
            if(shouldRenderGloves || shouldRenderHands)
            {
                if (_currentModel != EHandModel.Hand && shouldRenderHands)
                {
                    _renderer.gameObject.SetActive(true);
                    _currentModel = EHandModel.Hand;
                    SetGloves(false);
                }
                else if (_currentModel != EHandModel.Glove && shouldRenderGloves)
                {
                    _renderer.gameObject.SetActive(true);
                    _currentModel = EHandModel.Glove;
                    SetGloves(true);
                }
            }
            else if(_currentModel != EHandModel.Disabled)
            {
                _currentModel = EHandModel.Disabled;
                _renderer.gameObject.SetActive(false);
            }
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
