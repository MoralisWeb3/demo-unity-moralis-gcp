using UnityEngine;

public class ServerConfiguration : MonoBehaviour
{
    // Base URL
    public static readonly string URL = "Your Custom Backend URL";

    // Endpoints
    public static readonly string RequestEndpoint = "/request";
    public static readonly string VerifyEndpoint = "/verify";
    public static readonly string NativeBalanceEndpoint = "/nativeBalance";
}
