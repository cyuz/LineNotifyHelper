# LineNotifyHelper
A c# tool to wrap API for Line Notify

## Usage
### Send Message to Engineer access token (For developers)
1. just instantiate a empty LineNotifyOptions and IHttpClientFactory
```
IOptions<LineNotifyOptions> optionParameter = Options.Create(new LineNotifyOptions());

IHttpClientFactory clinetFatory = new ServiceCollection()
	.AddHttpClient()
	.BuildServiceProvider()
	.GetService<IHttpClientFactory>();
```
2. instantiate LineNotifySender with LineNotifyOptions and IHttpClientFactory
```
LineNotifySender sender = new LineNotifySender(optionParameter, clinetFatory);
```
3. call NotifyMessageAsync with token and message
```
string access_token = "YOUR_ACCESS_TOKEN";

var result = sender.NotifyMessageAsync(access_token, "Hello World!!").Result;
```
4. to send sticker[https://developers.line.biz/en/docs/messaging-api/sticker-list/#sticker-definitions]
```
string access_token = "YOUR_ACCESS_TOKEN";

var result = sender.NotifyMessageAsync(access_token, "Hello World!!", PackageID, StickerID).Result;
```

### General Use(send message to registered users)
1. acquire client_id/client_secrect from line notify offical page, and setup callbackurl correctly(e.g. https://localhost:5000/Home/Parse)
2. modify appsettings.json
```
  "LineNotifyOptions": {
    "ClientId": "YOUR_CLIEND_ID",
    "ClinetSecret": "YOUR_CLIENT_SECRET",
    "CallbackUrl": "YOUR_CALLBACK_URL"
    //"BotBaseUrl": "https://notify-bot.line.me/",
    //"NotifyBaseUrl": "https://notify-api.line.me/",
    //"NamedClient" : "YOUR_HTTP_CLIENT_NAME"
  },
```
3. inject LineNotifySender to service
```
public void ConfigureServices(IServiceCollection services)
{
	/// LineNotifySender need HttpClientFactory
	services.AddHttpClient();
	services.AddOptions<LineNotifyOptions>().Bind(Configuration.GetSection("LineNotifyOptions"));
	services.AddScoped<ILineNotifySender, LineNotifySender>();

	services.AddControllersWithViews();
}
```
4. when a user click {connect line}, generate a secret and compose the url, then redirect to it
```
public IActionResult ConnectLine()
{
	string state = GenearteSecrectStringAndStore()
	string url = _notifySender.GenerateAuthorizeUrl(state);
	return Redirect(url);
}
```
5. when user agree, line will redirect the callbackurl with code & state, use authorize code to get access token, and keep access token
```
[HttpPost]
public async Task<IActionResult> Parse(string code, string state)
{
	int id = GetCurrentUserId();
	string secret = GetPreviousSecrect();

	/// check secret to avoid attack
	if (state != secret)
	{
		return RedirectToAction("Index");
	}
	
	/// in case user resend request
	ClearSecret();
	
	/// acquired access token
	LineResponse<GetAccessTokenResponse> response = await _notifySender.GetAccessTokenAsync(code);
	
	if(!response.Success)
	{
		return RedirectToAction("Index");
	}
	
	/// stroe accessToken with current User
	_repository.ConnectLine(id, response.Body.access_token);

	return RedirectToAction("Index");
}
```
6. for sending message to user, just retrieve access token and send to it
```
public async Task<IActionResult> Hello(int id)
{
	UserModel model = _repository.GetUser(id);
	LineResponse<LineBaseResponseBody> response = await _notifySender.NotifyMessageAsync(model.AccessToken, "Hello World");

	return RedirectToAction("Index");
}
```
7. or revoke user acess token
```
public async Task<IActionResult> Revoke(int id)
{
	UserModel model = _repository.GetUser(id);
	LineResponse<LineBaseResponseBody> response = await _notifySender.RevokeAuthorizationAsync(model.AccessToken);

	_logger.LogDebug(JsonSerializer.Serialize(response));

	/// if the access token has been revoked, the result will be 401 Unauthorized
	if (response.Success || response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
	{
		_repository.Revoke(id);
	}

	return RedirectToAction("Index");
}
```
### Other
1. if you need the callbackUrl is called via GET
```
string url = _notifySender.GenerateAuthorizeUrl(state, false);
```
2. if you have registered multiple callbackUrls, and you need different callbackUrl by condition
```
/// assign callbackUrl parameter when generate AuthorizeUrl
string url = _notifySender.GenerateAuthorizeUrl(state, true, YOUR_ANOTHER_CALLBACK);
```
```
/// and assign same callbackUrl in GetAccessTokenAsync
LineResponse<GetAccessTokenResponse> response = await _notifySender.GetAccessTokenAsync(code, YOUR_ANOTHER_CALLBACK);
```
3. to use named client, specify NamedClient parameter in setting
```
  "LineNotifyOptions": {
    "ClientId": "YOUR_CLIEND_ID",
    "ClinetSecret": "YOUR_CLIENT_SECRET",
    "CallbackUrl": "YOUR_CALLBACK_URL",
    "NamedClient" : "YOUR_HTTP_CLIENT_NAME"
  },
```
4. to check detail fail reason
```
if (!result.Success)
{
	/// Http Status Code
	Console.WriteLine(result.StatusCode);
	/// raw message status
	Console.WriteLine(result.ErrorBody.status);
	/// raw message conent
	Console.WriteLine(result.ErrorBody.message);
}
```


