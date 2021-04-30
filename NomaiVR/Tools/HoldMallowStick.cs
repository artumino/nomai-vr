using OWML.Utils;
using UnityEngine;

namespace NomaiVR
{
    internal class HoldMallowStick : NomaiVRModule<HoldMallowStick.Behaviour, HoldMallowStick.Behaviour.Patch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => PlayableScenes;

        public class Behaviour : MonoBehaviour
        {
            internal void Start()
            {

                var scale = Vector3.one * 0.75f;
                var stickController = Locator.GetPlayerBody().transform.Find("RoastingSystem").GetComponent<RoastingStickController>();

                // Move the stick forward while not pressing RT.
                stickController._stickMinZ = 1f;

                var stickRoot = stickController.transform.Find("Stick_Root/Stick_Pivot");
                stickRoot.localScale = scale;

                var holdStick = stickRoot.gameObject.AddComponent<Holdable>();
                holdStick.SetPositionOffset(new Vector3(-0.029f, -0.174f, -0.29f));
                holdStick.SetRotationOffset(Quaternion.Euler(-20f, 0, 0));
                holdStick.SetPoses(AssetLoader.Poses["holding_roastingstick_gloves"]);

                var mallow = stickRoot.Find("Stick_Tip/Mallow_Root").GetComponent<Marshmallow>();

                void EatMallow(Transform other)
                {
                    if (mallow.GetState() != Marshmallow.MallowState.Gone)
                    {
                        mallow.Eat();
                    }
                }

                void ReplaceMallow(Transform other)
                {
                    if (mallow.GetState() == Marshmallow.MallowState.Gone)
                    {
                        mallow.SpawnMallow(true);
                    }
                }

                bool ShouldRenderMallowClone()
                {
                    return stickController.enabled && mallow.GetState() == Marshmallow.MallowState.Gone;
                }

                // Eat mallow by moving it to player head.
                var eatDetector = mallow.gameObject.AddComponent<ProximityDetector>();
                eatDetector.Other = PlayerHelper.PlayerHead;
                eatDetector.MinDistance = 0.2f;
                eatDetector.OnEnter += EatMallow;

                // Hide arms that are part of the stick object.
                var meshes = stickRoot.Find("Stick_Tip/Props_HEA_RoastingStick");
                meshes.Find("RoastingStick_Arm").GetComponent<Renderer>().enabled = false;
                meshes.Find("RoastingStick_Arm_NoSuit").GetComponent<Renderer>().enabled = false;

                // Hold mallow in left hand for replacing the one in the stick.
                var mallowModel = mallow.transform.Find("Props_HEA_Marshmallow");
                var mallowClone = Instantiate(mallowModel);
                mallowClone.GetComponent<MeshRenderer>().material.color = Color.white;
                mallowClone.localScale = scale;

                var holdMallow = mallowClone.gameObject.AddComponent<Holdable>();
                holdMallow.SetPositionOffset(new Vector3(-0.0451f, -0.002f, 0.0097f));
                holdMallow.SetRotationOffset(Quaternion.Euler(47.865f, 97.12901f, 83.881f));
                holdMallow.SetPoses(AssetLoader.Poses["holding_marshmallow"]);
                holdMallow.IsOffhand = true;

                // Replace right hand mallow on proximity with left hand mallow.
                var replaceDetector = mallowClone.gameObject.AddComponent<ProximityDetector>();
                replaceDetector.Other = mallow.transform;
                replaceDetector.OnEnter += ReplaceMallow;

                // Render left hand mallow only when right hand mallow is not present.
                mallowClone.gameObject.AddComponent<ConditionalRenderer>().getShouldRender += ShouldRenderMallowClone;
            }

            public class Patch : NomaiVRPatch
            {
                public override void ApplyPatches()
                {
                    // Stop stick rotation animation.
                    Empty<RoastingStickController>("UpdateRotation");

                    // Prevent stick from moving on colliding with stuff.
                    Postfix<RoastingStickController>("CalculateMaxStickExtension", nameof(PostCalculateMaxStickExtension));
                }

                [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Result required for return passthrough")]
                private static float PostCalculateMaxStickExtension(float __result, float ____stickMaxZ)
                {
                    return ____stickMaxZ;
                }
            }
        }
    }
}