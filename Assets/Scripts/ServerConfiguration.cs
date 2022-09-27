using UnityEngine;

public class ServerConfiguration : MonoBehaviour
{
    // Base URL
    public static readonly string URL = "https://hello-world-1-sxdda5kuea-uc.a.run.app";

    // Endpoints
    public static readonly string RequestEndpoint = "/request";
    public static readonly string VerifyEndpoint = "/verify";
    public static readonly string NativeBalanceEndpoint = "/nativeBalance";
}
