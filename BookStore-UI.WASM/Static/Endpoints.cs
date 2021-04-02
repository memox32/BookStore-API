using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStore_UI.WASM.Static
{
  public static class Endpoints
  {
    //just local, make it dynamic so it can be used in different environments
    public static string BaseUrl = "https://localhost:44335/";
    public static string AuthorsEndpoint = $"{BaseUrl}api/authors/";
    public static string BooksEndpoint = $"{BaseUrl}api/books/";
    public static string UserEndpoint = $"{BaseUrl}api/users/";
    public static string LoginEndpoint = $"{BaseUrl}api/users/login/";
  }
}
