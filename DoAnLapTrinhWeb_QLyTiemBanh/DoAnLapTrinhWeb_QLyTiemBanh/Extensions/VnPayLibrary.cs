
using System.Net;

namespace DoAnLapTrinhWeb_QLyTiemBanh.Extensions
{
    public class VnPayLibrary
    {
        public const string VERSION = "2.1.0";

        private readonly SortedList<string, string> _requestData = new SortedList<string, string>(new VnPayCompare());
        private readonly SortedList<string, string> _responseData = new SortedList<string, string>(new VnPayCompare());

        public void AddRequestData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _requestData[key] = value;
            }
        }

        public void AddResponseData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _responseData[key] = value;
            }
        }

        public string GetResponseData(string key)
        {
            return _responseData.TryGetValue(key, out var value) ? value : string.Empty;
        }

        public string CreateRequestUrl(string baseUrl, string vnp_HashSecret)
        {
            var query = string.Join("&", _requestData
                .Where(kv => !string.IsNullOrEmpty(kv.Value))
                .Select(kv => $"{WebUtility.UrlEncode(kv.Key)}={WebUtility.UrlEncode(kv.Value)}"));

            var signData = query;
            string vnp_SecureHash = Utils.HmacSHA512(vnp_HashSecret, signData);

            return $"{baseUrl}?{query}&vnp_SecureHash={vnp_SecureHash}";
        }

        public bool ValidateSignature(string inputHash, string secretKey)
        {
            string rawData = BuildRawResponseData();
            string computedHash = Utils.HmacSHA512(secretKey, rawData);
            return string.Equals(computedHash, inputHash, StringComparison.InvariantCultureIgnoreCase);
        }

        private string BuildRawResponseData()
        {
            // Loại bỏ các giá trị hash trước khi tính lại
            var filtered = _responseData
                .Where(kv => kv.Key != "vnp_SecureHash" && kv.Key != "vnp_SecureHashType")
                .Where(kv => !string.IsNullOrEmpty(kv.Value));

            var data = string.Join("&", filtered
                .Select(kv => $"{WebUtility.UrlEncode(kv.Key)}={WebUtility.UrlEncode(kv.Value)}"));

            return data;
        }
    }
}
