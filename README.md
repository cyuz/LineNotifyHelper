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
5. to check detail fail reason
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

### General Use
1. acquire client_id/client_secrect from line notify offical page, and setup callbackurl correctly(e.g. https://localhost:5000/Home/Parse)
2. modify appsettings.json
```
  "LineNotifyOptions": {
    "ClientId": "YOUR_CLIEND_ID",
    "ClinetSecret": "YOUR_CLIENT_SECRET",
    "CallbackUrl": "YOUR_CALLBACK_URL"
    //"BotBaseUrl": "https://notify-bot.line.me/",
    //"NotifyBaseUrl": "https://notify-api.line.me/",
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
	string url = _notifySender.GenerateAuthorizeUri(state);
	return Redirect(url);
}
```
5. if you need call the callback via GET
```
string url = _notifySender.GenerateAuthorizeUri(state, false);
```
6. when user agree, line will call the callback with code & state
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
7. to send message to user, just retrieve access token and send to it
```
public async Task<IActionResult> Hello(int id)
{
	UserModel model = _repository.GetUser(id);
	LineResponse<LineBaseResponseBody> response = await _notifySender.NotifyMessageAsync(model.AccessToken, "Hello World");

	return RedirectToAction("Index");
}
```
8. to revoke user acess token
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
