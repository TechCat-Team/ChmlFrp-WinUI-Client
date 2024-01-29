namespace ChmlFrp.Core.Models;

// Model for the SampleDataService. Replace with your own model.
public class TunnelInfo
{
    public int TunnelId
    {
        get; set;
    }

    public string IntranetInfo
    {
        get; set;
    }

    public string NodeInfo
    {
        get; set;
    }

    public string LinkAddress
    {
        get; set;
    }

    public string TunnelName
    {
        get; set;
    }
}

public class UserInfo
{
    public string UserName
    {
        get; set;
    }
    public string UserImage
    {
        get; set;
    }
    public string UserToken
    {
        get; set;
    }
    public string RealName
    {
        get; set;
    }
    public string Bandwidth
    {
        get; set;
    }
    public string Tunnel
    {
        get; set;
    }
    public string TunnelState
    {
        get; set;
    }
    public string Integral
    {
        get; set;
    }
    public string Term
    {
        get; set;
    }
    public string UserGroup
    {
        get; set;
    }
    public string QQ
    {
        get; set;
    }
    public string Email
    {
        get; set;
    }
    public string UserId
    {
        get; set;
    }

    public static UserInfo Get = new();
    public static string SettingsPath = "settings.json";
    public static bool TryLoadSettings()
    {
        bool result;
        if (result = File.Exists(SettingsPath))
        {
            var uInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<UserInfo>(File.ReadAllText(SettingsPath));
            if (result = (uInfo != null))
            {
                UserInfo.Get = uInfo;
            }
        }

        return result;
    }
    public static void SaveSettings()
    {
        File.WriteAllText(SettingsPath, Newtonsoft.Json.JsonConvert.SerializeObject(Get));
    }
}