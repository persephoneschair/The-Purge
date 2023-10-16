using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LobbyManager : SingletonMonoBehaviour<LobbyManager>
{
    public bool lateEntry;

    public TextMeshProUGUI codeMesh;

    public Animator lobbyCodeAnim;

    private const string permaMessage = "To join the game, visit <color=yellow>https://persephoneschair.itch.io/gamenight</color> and join with the room code <color=green>[ABCD]</color>";

    public Animator permaCodeAnim;
    public TextMeshProUGUI permaCodeMesh;

    [Button]
    public void OnOpenLobby()
    {
        PlayerManager.Get.UpdatePlayerCount();
        PlayerManager.Get.TogglePlayerCountStrap();
        AudioManager.Get.Play(AudioManager.LoopClip.Underscore);
        AudioManager.Get.Play(AudioManager.OneShotClip.Whoosh);
        lobbyCodeAnim.SetTrigger("toggle");
        codeMesh.text = $"Room code:\n<size=300%><color=yellow>{HostManager.Get.GetSpacedRoomCode()}</color></size>";
        HouseLightsManager.Get.ToggleAudienceLights();
    }

    [Button]
    public void OnLockLobby()
    {
        PlayerManager.Get.TogglePlayerCountStrap();
        AudioManager.Get.Play(AudioManager.OneShotClip.Whoosh);
        lateEntry = true;
        lobbyCodeAnim.SetTrigger("toggle");
        permaCodeMesh.text = permaMessage.Replace("[ABCD]", HostManager.Get.GetSpacedRoomCode());
        Invoke("TogglePermaCode", 1f);
        HouseLightsManager.Get.ToggleAudienceLights();
    }

    public void TogglePermaCode()
    {
        AudioManager.Get.Play(AudioManager.OneShotClip.Whoosh);
        permaCodeAnim.SetTrigger("toggle");
    }
}
