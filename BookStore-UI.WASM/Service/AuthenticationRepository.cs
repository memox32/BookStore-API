using Blazored.LocalStorage;
using BookStore_UI.WASM.Contracts;
using BookStore_UI.WASM.Models;
using BookStore_UI.WASM.Providers;
using BookStore_UI.WASM.Static;
using Microsoft.AspNetCore.Components.Authorization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace BookStore_UI.WASM.Service
{
  public class AuthenticationRepository : IAuthenticationRepository
  {

    private readonly HttpClient _client;
    private readonly ILocalStorageService _localStorage;
    private readonly AuthenticationStateProvider _authenticationStateProvider;

    public AuthenticationRepository(HttpClient client,
      ILocalStorageService localStorage,
      AuthenticationStateProvider authenticationStateProvider)
    {
      _client = client;
      _localStorage = localStorage;
      _authenticationStateProvider = authenticationStateProvider;
    }

    public async Task<bool> Login(LoginModel user)
    {
      var response = await _client.PostAsJsonAsync(Endpoints.LoginEndpoint, user);

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

      //setting the default request header, this is not necessary because we are already adding this to the call everytime
      _client.DefaultRequestHeaders.Authorization =
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
      var response = await _client.PostAsJsonAsync(Endpoints.UserEndpoint + "register/", user);

      return response.IsSuccessStatusCode;
    }
  }
}
