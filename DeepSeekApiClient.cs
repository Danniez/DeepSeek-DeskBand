using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace DeepSeekDeskBand
{
    /// <summary>
    /// DeepSeek API 客户端 —— 查询账户余额等
    /// </summary>
    public class DeepSeekApiClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private const string UserAgent = "DeepSeekDeskBand/1.0";

        // DeepSeek 余额查询 API 端点
        private const string BalanceUrl = "https://api.deepseek.com/user/balance";

        public DeepSeekApiClient()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(10)
            };
            _httpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent);
        }

        /// <summary>
        /// 查询账户余额
        /// </summary>
        /// <param name="apiKey">DeepSeek API Key</param>
        /// <returns>余额信息</returns>
        public async Task<BalanceResult> GetBalanceAsync(string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                return BalanceResult.NoApiKey();

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, BalanceUrl);
                request.Headers.Add("Authorization", $"Bearer {apiKey}");

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    string errorBody = await response.Content.ReadAsStringAsync();
                    return BalanceResult.Error(
                        $"API 请求失败 ({response.StatusCode})\n" +
                        (response.StatusCode == System.Net.HttpStatusCode.Unauthorized
                            ? "API Key 无效或已过期"
                            : errorBody.Length > 100 ? errorBody.Substring(0, 100) + "..." : errorBody));
                }

                string body = await response.Content.ReadAsStringAsync();
                return ParseBalanceResponse(body);
            }
            catch (TaskCanceledException)
            {
                return BalanceResult.Error("请求超时，请检查网络连接");
            }
            catch (HttpRequestException ex)
            {
                return BalanceResult.Error($"网络错误：{ex.Message}");
            }
            catch (Exception ex)
            {
                return BalanceResult.Error($"未知错误：{ex.Message}");
            }
        }

        /// <summary>
        /// 解析余额接口返回的 JSON
        /// </summary>
        private BalanceResult ParseBalanceResponse(string json)
        {
            try
            {
                var serializer = new JavaScriptSerializer();
                var root = serializer.Deserialize<Dictionary<string, object>>(json);

                bool isAvailable = false;
                if (root.TryGetValue("is_available", out var availObj))
                    bool.TryParse(availObj?.ToString(), out isAvailable);

                if (!root.TryGetValue("balance_infos", out var infosObj) || infosObj == null)
                    return BalanceResult.Ok(isAvailable, "—", "—", "—");

                var infos = infosObj as System.Collections.ArrayList;
                if (infos == null || infos.Count == 0)
                    return BalanceResult.Ok(isAvailable, "—", "—", "—");

                var info = infos[0] as Dictionary<string, object>;
                if (info == null)
                    return BalanceResult.Ok(isAvailable, "—", "—", "—");

                string GetStr(string key, string fallback = "—")
                {
                    return info.TryGetValue(key, out var v) ? v?.ToString() ?? fallback : fallback;
                }

                string currency = GetStr("currency", "CNY");
                string totalBalance = GetStr("total_balance");
                string grantedBalance = GetStr("granted_balance");
                string toppedUpBalance = GetStr("topped_up_balance");

                return BalanceResult.Ok(
                    isAvailable,
                    totalBalance,
                    grantedBalance,
                    toppedUpBalance,
                    currency);
            }
            catch (Exception ex)
            {
                return BalanceResult.Error($"Parse error: {ex.Message}");
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    /// <summary>
    /// 余额查询结果
    /// </summary>
    public class BalanceResult
    {
        public bool IsSuccess { get; private set; }
        public string? ErrorMessage { get; private set; }
        public string? TotalBalance { get; private set; }
        public string? GrantedBalance { get; private set; }
        public string? ToppedUpBalance { get; private set; }
        public string? Currency { get; private set; }
        public bool IsAvailable { get; private set; }

        // 状态枚举
        public BalanceStatus Status { get; private set; }

        public enum BalanceStatus
        {
            Ok,
            NoApiKey,
            ApiError,
            NetworkError,
            Unknown
        }

        private BalanceResult() { }

        public static BalanceResult Ok(
            bool isAvailable,
            string totalBalance,
            string grantedBalance,
            string toppedUpBalance,
            string currency = "CNY")
        {
            return new BalanceResult
            {
                IsSuccess = true,
                Status = BalanceStatus.Ok,
                IsAvailable = isAvailable,
                TotalBalance = totalBalance,
                GrantedBalance = grantedBalance,
                ToppedUpBalance = toppedUpBalance,
                Currency = currency
            };
        }

        public static BalanceResult NoApiKey()
        {
            return new BalanceResult
            {
                IsSuccess = false,
                Status = BalanceStatus.NoApiKey,
                ErrorMessage = "未设置 API Key\n右键 → 设置 API Key"
            };
        }

        public static BalanceResult Error(string message)
        {
            return new BalanceResult
            {
                IsSuccess = false,
                Status = message.Contains("超时") || message.Contains("网络")
                    ? BalanceStatus.NetworkError : BalanceStatus.ApiError,
                ErrorMessage = message
            };
        }
    }
}
