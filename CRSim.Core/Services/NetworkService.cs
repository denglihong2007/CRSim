using CRSim.Core.Abstractions;
using CRSim.Core.Models;
using CRSim.Core.Models.Plugin;
using System.Net;
using System.Text.Json;

namespace CRSim.Core.Services
{
    public class NetworkService() : INetworkService
    {
        public async Task<List<TrainStop>?> GetTimeTableAsync(string number)
        {
            try
            {
                HttpClient httpClient = new();
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/133.0.0.0 Safari/537.36 Edg/133.0.0.0");
                HttpContent content = new FormUrlEncodedContent(new Dictionary<string, string>()
                {
                    {"trainCode",number },
                    {"startDay",DateTime.Now.ToString("yyyyMMdd")},
                    {"startTime",""},
                    {"endDay",""},
                    {"endTime",""}
                });
                var response = await httpClient.PostAsync($"https://mobile.12306.cn/wxxcx/wechat/main/travelServiceQrcodeTrainInfo", content);

                if (response.IsSuccessStatusCode)
                {
                    var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                    var timeTable = new List<TrainStop>();
                    if (doc.RootElement.GetProperty("data").GetProperty("trainDetail").TryGetProperty("stopTime", out JsonElement jsonElement))
                    {
                        foreach (var item in jsonElement.EnumerateArray())
                        {
                            var stationName = item.GetProperty("stationName").GetString();
                            var arriveTimeStr = item.GetProperty("arriveTime").GetString();
                            var startTimeStr = item.GetProperty("startTime").GetString();
                            TimeSpan? arriveTime = ParseTime(arriveTimeStr);
                            TimeSpan? startTime = ParseTime(startTimeStr);
                            if (arriveTimeStr == startTimeStr)
                            {
                                if (timeTable.Count == 0)
                                {
                                    arriveTime = null;
                                }
                                else
                                {
                                    startTime = null;
                                }
                            }
                            timeTable.Add(new TrainStop
                            {
                                Station = stationName,
                                ArrivalTime = arriveTime,
                                DepartureTime = startTime
                            });
                        }
                    }
                    return timeTable;
                }
            }
            catch
            {
            }
            return null;
        }

        private static TimeSpan? ParseTime(string timeStr)
        {
            timeStr = string.Concat(timeStr.AsSpan(0, 2), ":", timeStr.AsSpan(2, 2));
            if (string.IsNullOrWhiteSpace(timeStr) || timeStr == "----")
                return null;
            if (TimeSpan.TryParseExact(timeStr, @"hh\:mm", null, out TimeSpan time))
            {
                return time;
            }
            return null;
        }

        public async Task<List<TrainStop>?> GetTrainNumbersAsync(string name)
        {
            try
            {
                var client = new HttpClient();
                var stations = (await client.GetStringAsync("https://kyfw.12306.cn/otn/resources/js/framework/station_name.js")).Split("|||");
                string tel = "";
                foreach (string station in stations)
                {
                    if (station.StartsWith("';")) continue;
                    if (station.Split("|")[1] == name)
                    {
                        tel = station.Split("|")[2];
                    }
                }
                if (tel == "")
                {
                    return [];
                }
                var baseAddress = new Uri("https://mobile.12306.cn/wxxcx/wechat/bigScreen/queryTrainByStation");
                var cookieContainer = new CookieContainer();
                cookieContainer.Add(baseAddress, new Cookie("BIGipServerweixin_xiaochengxu", "2028077578.5670.0000"));
                var handler = new HttpClientHandler() { CookieContainer = cookieContainer, UseCookies = true };

                HttpContent content = new FormUrlEncodedContent(new Dictionary<string, string>()
                {
                    {"train_station_code",tel },
                    {"train_start_date",DateTime.Now.ToString("yyyyMMdd")}
                });
                client = new HttpClient(handler) { BaseAddress = baseAddress };
                client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36 MicroMessenger/7.0.20.1781(0x6700143B) NetType/WIFI MiniProgramEnv/Windows WindowsWechat/WMPF WindowsWechat(0x63090c11)XWEB/11581");
                var response = await client.PostAsync(baseAddress, content);
                var json = await response.Content.ReadAsStringAsync();
                if (json.Contains("操作失败，请稍后重试")) return [];
                var list = JsonDocument.Parse(json).RootElement.GetProperty("data").EnumerateArray();
                if (!list.Any()) return [];
                List<TrainStop> trainStops = [];
                foreach (var item in list)
                {
                    var arriveTimeStr = item.GetProperty("arrive_time").GetString();
                    var startTimeStr = item.GetProperty("start_time").GetString();
                    TimeSpan? arriveTime = null;
                    TimeSpan? startTime = null;
                    if (arriveTimeStr != "----")
                    {
                        arriveTime = TimeSpan.Parse(arriveTimeStr);
                    }
                    if (startTimeStr != arriveTimeStr)
                    {
                        startTime = TimeSpan.Parse(startTimeStr);
                    }
                    trainStops.Add(new TrainStop() { Number = item.GetProperty("station_train_code").ToString(), Terminal = item.GetProperty("end_station_name").ToString(), Origin = item.GetProperty("start_station_name").ToString(), ArrivalTime = arriveTime, DepartureTime = startTime });
                }
                return trainStops;
            }
            catch
            {
            }
            return null;
        }

        public List<PluginManifest>? GetOnlinePlugins(string url)
        {
            try
            {
                var client = new HttpClient();
                var response = client.GetStringAsync(url).Result;
                return JsonSerializer.Deserialize(response, JsonContextWithCamelCase.Default.ListPluginManifest);
            }
            catch
            {
            }
            return null;
        }

        public async Task<UpdateInfo?> GetUpdateAsync(string uri)
        {
            try
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.UserAgent.ParseAdd("CRSim");
                var response = await client.GetStringAsync(uri);
                return JsonSerializer.Deserialize(response, JsonContext.Default.UpdateInfo);
            }
            catch
            {
            }
            return null;
        }
    }
}
