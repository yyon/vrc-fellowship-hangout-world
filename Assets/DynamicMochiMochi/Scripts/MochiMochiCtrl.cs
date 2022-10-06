
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class MochiMochiCtrl : UdonSharpBehaviour
{
    [SerializeField] Texture2D m_HeightTexPancake;
    [SerializeField] Texture2D m_HeightTexJelly;
    [SerializeField] Texture2D m_HeightTexPudding;
    [SerializeField] GameObject m_Pancake;
    [SerializeField] GameObject m_Jelly;
    [SerializeField] GameObject m_Pudding;
    [SerializeField] Material m_DynamicMat;

    [UdonSynced(UdonSyncMode.None)] int sync_mochi_id;

    void Start()
    {
        m_Pancake.SetActive(true);
        m_DynamicMat.SetTexture("_HeightTex", m_HeightTexPancake);
        m_DynamicMat.SetFloat("_stiffness", 0.8f);
        m_DynamicMat.SetFloat("_elasticity", 0.04f);
        m_DynamicMat.SetFloat("_damping", 0.3f);
        sync_mochi_id = 0;

        m_Jelly.SetActive(false);
        m_Pudding.SetActive(false);
    }

    private void ToggleMochiMochi(int id)
    {
        if (id == 0)
        {
            m_Pancake.SetActive(true);
            m_DynamicMat.SetTexture("_HeightTex", m_HeightTexPancake);
            m_DynamicMat.SetFloat("_stiffness", 0.8f);
            m_DynamicMat.SetFloat("_elasticity", 0.04f);
            m_DynamicMat.SetFloat("_damping", 0.3f);

            m_Jelly.SetActive(false);
            m_Pudding.SetActive(false);
        }
        else if (id == 1)
        {
            m_Pancake.SetActive(false);

            m_Jelly.SetActive(true);
            m_DynamicMat.SetTexture("_HeightTex", m_HeightTexJelly);
            m_DynamicMat.SetFloat("_stiffness", 0.85f);
            m_DynamicMat.SetFloat("_elasticity", 0.04f);
            m_DynamicMat.SetFloat("_damping", 0.02f);

            m_Pudding.SetActive(false);
        }
        else if (id == 2)
        {
            m_Pancake.SetActive(false);
            m_Jelly.SetActive(false);

            m_Pudding.SetActive(true);
            m_DynamicMat.SetTexture("_HeightTex", m_HeightTexPudding);
            m_DynamicMat.SetFloat("_stiffness", 0.85f);
            m_DynamicMat.SetFloat("_elasticity", 0.04f);
            m_DynamicMat.SetFloat("_damping", 0.02f);
        }
    }

    public override void OnDeserialization()
    {
        if (!Networking.IsOwner(Networking.LocalPlayer, this.gameObject))
        {
            ToggleMochiMochi(sync_mochi_id);
        }
    }

    public void ActivatePancake()
    {
        var player = Networking.LocalPlayer;
        if (player != null && player.IsValid())
        {
            Networking.SetOwner(player, this.gameObject);

            if (player.IsOwner(this.gameObject))
            {
                sync_mochi_id = 0;
                SendCustomEventDelayedSeconds(nameof(SerializeData), 0.2f);
                ToggleMochiMochi(sync_mochi_id);
            }
        }
    }

    public void ActivateJelly()
    {
        var player = Networking.LocalPlayer;
        if (player != null && player.IsValid())
        {
            Networking.SetOwner(player, this.gameObject);

            if (player.IsOwner(this.gameObject))
            {
                sync_mochi_id = 1;
                SendCustomEventDelayedSeconds(nameof(SerializeData), 0.2f);
                ToggleMochiMochi(sync_mochi_id);
            }
        }
    }

    public void ActivatePudding()
    {
        var player = Networking.LocalPlayer;
        if (player != null && player.IsValid())
        {
            Networking.SetOwner(player, this.gameObject);

            if (player.IsOwner(this.gameObject))
            {
                sync_mochi_id = 2;
                SendCustomEventDelayedSeconds(nameof(SerializeData), 0.2f);
                ToggleMochiMochi(sync_mochi_id);
            }
        }
    }

    public void SerializeData()
    {
        RequestSerialization();
    }
}
