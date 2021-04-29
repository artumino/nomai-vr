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
                scroll.SetPositionOffset(new Vector3(0.02f, -0.04f, -0.03f));
                scroll.SetRotationOffset(Quaternion.Euler(321.2f, 104f, 194f));

                var stone = _itemTool.transform.Find("SharedStoneSocket").gameObject.AddComponent<Holdable>();
                stone.SetPositionOffset(new Vector3(-0.05f, -0.01f, 0f));
                stone.SetRotationOffset(Quaternion.Euler(-22.8f, 0f, 0f));

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