using MoralisUnity.Kits.AuthenticationKit;
using StarterAssets;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public AuthenticationKit authenticationKit;
    public StarterAssetsInputs starterAssetsInput;
    public Text menuText;

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

    public void OpenMenu()
    {
        authenticationKit.gameObject.SetActive(true);
        starterAssetsInput.EnableInput(false);

        menuText.text = "Press 'M' to close Menu";
    }

    public void CloseMenu()
    {
        authenticationKit.gameObject.SetActive(false);
        starterAssetsInput.EnableInput(true);

        menuText.text = "Press 'M' to open Menu";
    }
}
