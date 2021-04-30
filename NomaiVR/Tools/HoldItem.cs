using UnityEngine;

namespace NomaiVR
{
    internal class HoldItem : NomaiVRModule<HoldItem.Behaviour, NomaiVRModule.EmptyPatch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => PlayableScenes;

        public class Behaviour : MonoBehaviour
        {
            private ItemTool _itemTool;

            internal void Start()
            {
                _itemTool = FindObjectOfType<ItemTool>();
                _itemTool.transform.localScale = 1.8f * Vector3.one;

                _itemTool.transform.Find("ItemSocket").gameObject.AddComponent<Holdable>();
                _itemTool.transform.Find("LanternSocket").gameObject.AddComponent<Holdable>();

                var scroll = _itemTool.transform.Find("ScrollSocket").gameObject.AddComponent<Holdable>();
                scroll.SetPositionOffset(new Vector3(-0.022f, -0.033f, -0.03f), new Vector3(-0.0436f, -0.033f, -0.03f));
                scroll.SetRotationOffset(Quaternion.Euler(352.984f, 97.98601f, 223.732f));
                scroll.SetPoses(AssetLoader.Poses["holding_scroll_gloves"], AssetLoader.Poses["holding_scroll_gloves"]);

                var stone = _itemTool.transform.Find("SharedStoneSocket").gameObject.AddComponent<Holdable>();
                stone.SetPositionOffset(new Vector3(-0.1139f, -0.0041f, 0.0193f));
                stone.SetRotationOffset(Quaternion.Euler(-22.8f, 0f, 0f));
                stone.SetPoses(AssetLoader.Poses["holding_sharedstone"], AssetLoader.Poses["holding_sharedstone_gloves"]);

                var warpCore = _itemTool.transform.Find("WarpCoreSocket").gameObject.AddComponent<Holdable>();
                warpCore.SetPositionOffset(new Vector3(-0.06f, -0.07f, -0.05f));
                warpCore.SetRotationOffset(Quaternion.Euler(276.2f, 49f, 104f));

                var vesselCore = _itemTool.transform.Find("VesselCoreSocket").gameObject.AddComponent<Holdable>();
                vesselCore.SetPositionOffset(new Vector3(-0.01f, 0.03f, 0.01f));
                vesselCore.SetRotationOffset(Quaternion.Euler(-1.7f, 70.4f, 26f));
            }

            private void SetActive(bool active)
            {
                var heldItem = _itemTool.GetHeldItem();
                if (!heldItem)
                {
                    return;
                }
                heldItem.gameObject.SetActive(active);
            }

            private bool IsActive()
            {
                var heldItem = _itemTool.GetHeldItem();
                if (!heldItem)
                {
                    return false;
                }
                return heldItem.gameObject.activeSelf;
            }

            internal void Update()
            {
                if (IsActive() && ToolHelper.IsUsingAnyTool())
                {
                    SetActive(false);
                }
                else if (!IsActive() && !ToolHelper.IsUsingAnyTool())
                {
                    SetActive(true);
                }
            }
        }
    }
}