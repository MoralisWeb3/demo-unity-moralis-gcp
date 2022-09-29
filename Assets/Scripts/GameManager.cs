using MoralisUnity.Kits.AuthenticationKit;
using StarterAssets;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using WalletConnectSharp.Unity;

public class GameManager : MonoBehaviour
{
    [Header("Moralis")]
    public AuthenticationKit authenticationKit;

    [Header("Player")]
    public PlayerController playerController;

    [Header("HUD")]
    public Text menuLabel;
    public Text nativeBalanceLabel;

    private void Update()
    {
        // Press "M" to open Menu
        if (Input.GetKeyDown(KeyCode.M))
        {
            // We check either we activate the AuthenticationKit or deactivate it
            if (authenticationKit.gameObject.activeSelf)
            {
                // We don't want to deactivate AuthenticationKit while we're in the connecting process.
                if (authenticationKit.State == AuthenticationKitState.Initializing ||
                    authenticationKit.State == AuthenticationKitState.WalletConnecting ||
                    authenticationKit.State == AuthenticationKitState.WalletSigning ||
                    authenticationKit.State == AuthenticationKitState.Disconnecting) return;

                CloseMenu();
            }
            else
            {
                OpenMenu();     
            }
        }
    }

    public void OnAuthenticatedSuccessfully()
    {
#if !UNITY_WEBGL
        // Get the address and chainid with WalletConnect 

        // TODO Internal Note: If we contiue to offer the AuthenticationKit,
        // it would be nice to have something like authenticationKit.getWalletAddress()
        string address = WalletConnect.ActiveSession.Accounts[0];
        int chainId = WalletConnect.ActiveSession.ChainId;
#else
        // Get the address and chainid with Web3 
        string address = Web3GL.Account().ToLower();
        int chainId = Web3GL.ChainId();
#endif

        playerController.walletAddress.Show(address);
        StartCoroutine(GetNativeBalance(address, chainId));
        CloseMenu();
    }

    public void OnDisconnected()
    {
        playerController.walletAddress.Hide();
        nativeBalanceLabel.text = "0";
        CloseMenu();
    }

    public void OpenMenu()
    {
        authenticationKit.gameObject.SetActive(true);
        playerController.input.EnableInput(false);

        menuLabel.text = "Press 'M' to close Menu";
    }

    public void CloseMenu()
    {
        authenticationKit.gameObject.SetActive(false);
        playerController.input.EnableInput(true);

        menuLabel.text = "Press 'M' to open Menu";
    }

    IEnumerator GetNativeBalance(string address, int chainId)
    {
        var hexChainId = $"0x{chainId:X}";

        WWWForm form = new WWWForm();
        form.AddField("address", address);
        form.AddField("chain", hexChainId);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(
            ServerConfiguration.URL + ServerConfiguration.NativeBalanceEndpoint, form))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log(webRequest.downloadHandler.text);
                nativeBalanceLabel.text = webRequest.downloadHandler.text;
            }
            else
            {
                Debug.LogError("Error: " + webRequest.error);
            }
        }
    }
}
