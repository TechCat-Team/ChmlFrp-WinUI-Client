using ChmlFrp.Core.Contracts.Services;
using ChmlFrp.Core.Models;
using System.Net.Http;
using Newtonsoft.Json;

namespace ChmlFrp.Core.Services;

public class SampleDataService : ISampleDataService
{
    private List<TunnelInfo> _allOrders;
    private readonly HttpClient _httpClient;

    public SampleDataService()
    {
        _httpClient = new HttpClient();
    }

    private static IEnumerable<TunnelInfo> AllOrders()
    {
        // The following is order summary data
        var companies = AllCompanies();
        return companies.SelectMany(c => c.Orders);
    }

    // 创建用于调用API的方法
    private async Task<IEnumerable<TunnelInfo>> FetchOrdersFromApiAsync()
    {
        var userToken = UserInfo.UserToken;
        var apiUrl = $"https://panel.chmlfrp.cn/api/usertunnel.php?token={userToken}";

        var response = await _httpClient.GetStringAsync(apiUrl);
        var apiOrders = JsonConvert.DeserializeObject<List<ApiOrder>>(response);

        // 将API返回的数据转换为SampleOrder类型
        var sampleOrders = apiOrders.Select(apiOrder => new TunnelInfo
        {
            TunnelId = int.Parse(apiOrder.Id),
            TunnelName = apiOrder.Name,
            IntranetInfo = $"{apiOrder.Localip}:{apiOrder.Nport} - {apiOrder.Type.ToUpper()}",
            NodeInfo = apiOrder.Node,
            LinkAddress = apiOrder.Ip,
        });

        return sampleOrders;
    }

    public async Task<IEnumerable<TunnelInfo>> GetContentGridDataAsync()
    {
        // 调用API以获取最新的数据
        var apiOrders = await FetchOrdersFromApiAsync();

        // 更新本地数据
        _allOrders = new List<TunnelInfo>(apiOrders);

        return _allOrders;
    }

    // 内部类，用于解析API返回的数据
    private class ApiOrder
    {
        public string Id
        {
            get; set;
        }
        public string Name
        {
            get; set;
        }
        public string Localip
        {
            get; set;
        }
        public string Type
        {
            get; set;
        }
        public string Nport
        {
            get; set;
        }
        public string Node
        {
            get; set;
        }
        public string Ip
        {
            get; set;
        }
    }

    private static IEnumerable<SampleCompany> AllCompanies()
    {
        return new List<SampleCompany>()
        {
            new()
            {
                Orders = new List<TunnelInfo>()
                {
                    new()
                    {
                        TunnelId = 10000,
                        TunnelName = "隧道名",
                        IntranetInfo = "本地ip",
                        NodeInfo = "节点数据",
                        LinkAddress = "链接地址",
                    },
                }
            },
        };
    }
}
