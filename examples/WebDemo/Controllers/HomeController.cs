using LineNotifyHelper;
using LineNotifyHelper.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using WebDemo.Models;
using WebDemo.Repository;

namespace WebDemo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserRepository _repository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILineNotifySender _notifySender;

        private int? SessionUserId
        {
            get
            {
                return _httpContextAccessor.HttpContext.Session.GetInt32("UserId");
            }
            set
            {
                if(value.HasValue)
                {
                    _httpContextAccessor.HttpContext.Session.SetInt32("UserId", value.Value);
                }
                else
                {
                    _httpContextAccessor.HttpContext.Session.Remove("UserId");
                }
            }
        }

        private string SessionSecret
        {
            get
            {
                return _httpContextAccessor.HttpContext.Session.GetString("state_guid");
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _httpContextAccessor.HttpContext.Session.Remove("state_guid");
                }
                else
                {
                    _httpContextAccessor.HttpContext.Session.SetString("state_guid", value);
                }
            }
        }

        

        public HomeController(UserRepository repository, IHttpContextAccessor httpContextAccessor, ILineNotifySender notifySender, ILogger<HomeController> logger)
        {
            _repository = repository;
            _httpContextAccessor = httpContextAccessor;
            _notifySender = notifySender;
            _logger = logger;
        }

        public IActionResult Index()
        {
            List<UserModel> users = _repository.GetAllUsers();

            int? id = this.SessionUserId;

            if(id.HasValue)
            {
                ViewBag.UserInfo = _repository.GetUser(id.Value);
            }

            return View(users);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Login([FromForm]string loginName)
        {
            if(!string.IsNullOrEmpty(loginName))
            {
                UserModel model = _repository.Login(loginName);

                this.SessionUserId = model.Id;
            }

            return RedirectToAction("Index");
        }

        public IActionResult ConnectLine()
        {
            int? id = this.SessionUserId;

            if(!id.HasValue)
            {
                return RedirectToAction("Index");
            }

            string secrectString = Guid.NewGuid().ToString();
            this.SessionSecret = secrectString;

            string state = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(id + "/" + secrectString));

            string url = _notifySender.GenerateAuthorizeUrl(state);
            return Redirect(url);
        }

        [HttpPost]
        public async Task<IActionResult> Parse(string code, string state)
        {
            string[] decodedState = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(state)).Split("/");

            int? id = this.SessionUserId;
            string secret = this.SessionSecret;

            if (id != int.Parse(decodedState[0]) && secret != decodedState[1])
            {
                return RedirectToAction("Index");
            }
            
            this.SessionSecret = null;

            LineResponse<GetAccessTokenResponse> response = await _notifySender.GetAccessTokenAsync(code);

            _logger.LogDebug(JsonSerializer.Serialize(response));

            if(!response.Success)
            {
                return RedirectToAction("Index");
            }

            _repository.ConnectLine(id.Value, response.Body.access_token);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> RevokeSelf()
        {
            int? id = this.SessionUserId;

            if (!id.HasValue)
            {
                return RedirectToAction("Index");
            }

            UserModel model = _repository.GetUser(id.Value);
            LineResponse<LineBaseResponseBody> response = await _notifySender.RevokeAuthorizationAsync(model.AccessToken);

            _logger.LogDebug(JsonSerializer.Serialize(response));

            if (response.Success)
            {
                _repository.Revoke(id.Value);
            }

            return RedirectToAction("Index");
        }

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

        public async Task<IActionResult> Hello(int id)
        {
            UserModel model = _repository.GetUser(id);
            LineResponse<LineBaseResponseBody> response = await _notifySender.NotifyMessageAsync(model.AccessToken, "Hello World");

            _logger.LogDebug(JsonSerializer.Serialize(response));

            return RedirectToAction("Index");
        }

        public IActionResult LogOut()
        {
            this.SessionUserId = null;

            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
