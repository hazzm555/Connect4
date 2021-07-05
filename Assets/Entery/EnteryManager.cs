using UnityEngine.SceneManagement;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class EnteryManager : MonoBehaviourPunCallbacks
{
    public GameObject OnlineButton;
    public InputField NameHolder;

    private void Awake()
    {
        NameHolder.text = DataSaver.Name;
    }

    public void Play()
    {
        SceneManager.LoadScene("FirstDay");
    }


    public void PlayOnline()
    {
        DataSaver.Name = NameHolder.text;
        OnlineButton.SetActive(false);
        PhotonNetwork.ConnectUsingSettings();
    }

    public void Exit()
    {
        Application.Quit();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected");
        SceneManager.LoadScene("MatchingUp");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("Reason to failed  = " + cause.ToString());
        OnlineButton.SetActive(true);
    }
    



}
