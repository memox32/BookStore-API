using Blazored.LocalStorage;
using BookStore_UI.Contracts;
using BookStore_UI.Models;
using BookStore_UI.Providers;
using BookStore_UI.Static;
using Microsoft.AspNetCore.Components.Authorization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace BookStore_UI.Service
{
  public class AuthenticationRepository : IAuthenticationRepository
  {

    private readonly IHttpClientFactory _client;
    private readonly ILocalStorageService _localStorage;
    private readonly AuthenticationStateProvider _authenticationStateProvider;

    public AuthenticationRepository(IHttpClientFactory client,
      ILocalStorageService localStorage,
      AuthenticationStateProvider authenticationStateProvider)
    {
      _client = client;
      _localStorage = localStorage;
      _authenticationStateProvider = authenticationStateProvider;
    }

    public async Task<bool> Login(LoginModel user)
    {
      var request = new HttpRequestMessage(HttpMethod.Post, Endpoints.UserEndpoint + "login/");
      request.Content = new StringContent(JsonConvert.SerializeObject(user),
         Encoding.UTF8, "application/json");

      var client = _client.CreateClient();
      HttpResponseMessage response = await client.SendAsync(request);

      if (!response.IsSuccessStatusCode)
      {
        return false;
      }

      var content = await response.Content.ReadAsStringAsync();
      var token = JsonConvert.DeserializeObject<TokenResponse>(content);

      //store the token
      await _localStorage.SetItemAsync("authToken", token.Token);

      //change the authorization state of the application
      //we need to call this to do the login
      await ((APIAuthenticationStateProvider)_authenticationStateProvider).LogginIn();

      //setting the default request header
      client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("bearer", token.Token);
      return true;
    }

    public async Task Logout()
    {
      await _localStorage.RemoveItemAsync("authToken");
      ((APIAuthenticationStateProvider)_authenticationStateProvider).Logout();
    }

    public async Task<bool> Register(RegistrationModel user)
    {

      //var request = new HttpRequestMessage(HttpMethod.Post, Endpoints.RegisterEndpoint + "register/");
      /*
       //var request = new HttpRequestMessage(HttpMethod.Post, Endpoints.RegisterEndpoint + "register/");
      var request = new HttpRequestMessage(HttpMethod.Post, Endpoints.RegisterEndpoint)
      {
        Content = new StringContent(JsonConvert.SerializeObject(user),
         Encoding.UTF8, "application/json")
      };
       */
      var request = new HttpRequestMessage(HttpMethod.Post, Endpoints.UserEndpoint + "register/");
      request.Content = new StringContent(JsonConvert.SerializeObject(user),
         Encoding.UTF8, "application/json");

      var client = _client.CreateClient();
      HttpResponseMessage response = await client.SendAsync(request);

      return response.IsSuccessStatusCode;
    }
  }
}
