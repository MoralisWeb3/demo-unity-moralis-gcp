using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.Events;
using WalletConnectSharp.Unity;
using Cysharp.Threading.Tasks;

using MoralisUnity.Kits.AuthenticationKit;

public class MoralisWeb3AuthService : MonoBehaviour
{
    [Serializable]
    public class RequestData
    {
        public string id;
        public string message;
        public string profileId;
    }

    private RequestData requestData;

    [Header("Server Information")]
    [SerializeField] private string baseUrl;

    [Header("Events")]
    public UnityEvent OnSuccess = new UnityEvent();
    public UnityEvent OnFailed = new UnityEvent();

    // Endpoints
    private const string RequestEndpoint = "/request";
    private const string VerifyEndpoint = "/verify";

    // Main Components
    private AuthenticationKit authenticationKit;
    
    public void Awake()
    {
        authenticationKit = FindObjectOfType<AuthenticationKit>(true);
    }

    public void StateObservable_OnValueChanged(AuthenticationKitState authenticationKitState)
    {
        switch (authenticationKitState)
        {
            case AuthenticationKitState.WalletConnected:

#if !UNITY_WEBGL
                // Get the address and chainid with WalletConnect 
                string address = WalletConnect.ActiveSession.Accounts[0];
                int chainId = WalletConnect.ActiveSession.ChainId;
#else
                // Get the address and chainid with Web3 
                string address = Web3GL.Account().ToLower();
                int chainId = Web3GL.ChainId();
#endif
                // Create sign message 
                StartCoroutine(CreateMessage(address, chainId));
                break;
        }
    }

    IEnumerator CreateMessage(string address, int chainId)
    {
        var hexChainId = $"0x{chainId:X}";

        WWWForm form = new WWWForm();
        form.AddField("address", address);
        form.AddField("chain", hexChainId);
         
        using (UnityWebRequest webRequest = UnityWebRequest.Post(baseUrl + RequestEndpoint, form))
        {
            yield return webRequest.SendWebRequest();

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                    Debug.LogError("Connection Error: " + webRequest.error);
                    OnFailed?.Invoke();
                    break;

                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError("Data Processing Error: " + webRequest.error);
                    OnFailed?.Invoke();
                    break;

                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError("Protocol Error: " + webRequest.error);
                    OnFailed?.Invoke();
                    break;

                case UnityWebRequest.Result.Success:
                    requestData = JsonUtility.FromJson<RequestData>(webRequest.downloadHandler.text);
                    Debug.Log(requestData.message);

                    SignMessage(requestData.message);
                    break;
            }
        }
    }

    private async void SignMessage(string message)
    {
        authenticationKit.State = AuthenticationKitState.WalletSigning;

#if !UNITY_WEBGL
        // Sign the message with WalletConnect
        string address = WalletConnect.ActiveSession.Accounts[0];
        string signature = await WalletConnect.ActiveSession.EthPersonalSign(address, message);
#else
        // Sign the message with Web3
        string signature = await Web3GL.Sign(message);
#endif
        if (!String.IsNullOrEmpty(signature))
        {
            StartCoroutine(Authenticate(message, signature));
        }
        else
        {
            // If there is no signature fire the OnFailed event
            OnFailed.Invoke();
        }
    }

    IEnumerator Authenticate(string message, string signature)
    {
        WWWForm form = new WWWForm();
        form.AddField("message", message);
        form.AddField("signature", signature);

        using (UnityWebRequest webRequest = UnityWebRequest.Post(baseUrl + VerifyEndpoint, form))
        {
            yield return webRequest.SendWebRequest();

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                    Debug.LogError("Connection Error: " + webRequest.error);
                    OnFailed?.Invoke();
                    break;

                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError("Data Processing Error: " + webRequest.error);
                    OnFailed?.Invoke();
                    break;

                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError("Protocol Error: " + webRequest.error);
                    OnFailed?.Invoke();
                    break;

                case UnityWebRequest.Result.Success:

                    // If the authentication succeeded the user profile is update and we get the UpdateUserDataAsync return values a response
                    // If it failed it returns empty
                    if (!String.IsNullOrEmpty(webRequest.downloadHandler.text))
                    {
                        authenticationKit.State = AuthenticationKitState.WalletSigned;

                        // On success fire the OnSuccess event
                        OnSuccess.Invoke();
                        Debug.Log(webRequest.downloadHandler.text);
                    }
                    else
                    {
                        // If the response is empty fire the OnFailed event
                        OnFailed.Invoke();
                    }

                    break;
            }
        }
    }
}