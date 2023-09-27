using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class MatrixService
{
    private readonly HttpClient _client;
    private readonly string _baseUrl;
    private string _accessToken;

    public MatrixService(string baseUrl)
    {
        _baseUrl = baseUrl;
        _client = new HttpClient();
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        var content = new JObject
        {
            ["type"] = "m.login.password",
            ["user"] = username,
            ["password"] = password
        };

        var response = await _client.PostAsync(
            $"{_baseUrl}/_matrix/client/r0/login ",
            new StringContent(content.ToString(), Encoding.UTF8, "application/json"));

        if (response.IsSuccessStatusCode)
        {
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var tokenResponse = JObject.Parse(jsonResponse);
            _accessToken = tokenResponse["access_token"].ToString();
            return true;
        }

        return false;
    }

    public async Task<bool> SendMessageAsync(string roomId, string message)
    {
        if (string.IsNullOrWhiteSpace(_accessToken))
            throw new InvalidOperationException("You must be logged in to send a message.");

        var txnId = Guid.NewGuid().ToString();
        var content = new JObject
        {
            ["msgtype"] = "m.text",
            ["body"] = message
        };

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

        var response = await _client.PutAsync(
            $"{_baseUrl}/_matrix/client/r0/rooms/{roomId}/send/m.room.message/{txnId}",
            new StringContent(content.ToString(), Encoding.UTF8, "application/json"));

        return response.IsSuccessStatusCode;
    }

    public async Task<string> CreateRoomAsync(string roomName, string roomAlias)
    {
        var content = new
        {
            room_alias_name = roomAlias,
            name = roomName,
            visibility = "public"
        };

        var response = await _client.PostAsync(
            $"{_baseUrl}/_matrix/client/r0/createRoom?access_token={_accessToken}",
            new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, "application/json"));

        if (response.IsSuccessStatusCode)
        {
            var responseData = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());
            return responseData.room_id;
        }

        return null;
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
    {
        using var content = new StreamContent(fileStream);
        content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        var response = await _client.PostAsync(
            $"{_baseUrl}/_matrix/media/r0/upload?access_token={_accessToken}",
            content);

        if (response.IsSuccessStatusCode)
        {
            var responseData = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());
            return responseData.content_uri;
        }

        return null;
    }

    public async Task<bool> SendFileMessageAsync(string roomId, string mediaUrl, string fileName)
    {
        var txnId = Guid.NewGuid().ToString();
        var content = new
        {
            msgtype = "m.file",
            body = fileName,
            url = mediaUrl
        };

        var response = await _client.PutAsync(
            $"{_baseUrl}/_matrix/client/r0/rooms/{roomId}/send/m.room.message/{txnId}?access_token={_accessToken}",
            new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, "application/json"));

        return response.IsSuccessStatusCode;
    }
}
