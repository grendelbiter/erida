using Assets.Wulfram3.Scripts.InternalApis.Classes;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Net.Http;
using System.Net;
using System;
using System.Net.Http.Headers;
using System.Text;

public class Api
{
    public string Url { get { return "http://www.wulfrida.com"; } }

    //public string Url { get { return "http://localhost:8080"; } }

    public async Task<bool> Startup()
    {
        try
        {
            using (var client = this.CreateClient())
            {
                var response = await client.GetAsync("/");
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        catch (Exception)
        {

            return false;
        }
    }

    public async Task<ApiResult<WulframPlayer>> Login(string username, string password)
    {
        var result = await this.MakePostRequest<WulframPlayer>("/api/v1/player/login", new { username = username, password = password });
        return result;
    }

    public async Task<ApiResult<bool>> Logout(string userId)
    {
        var result = await this.MakePostRequest<bool>("/api/v1/player/logout", new { Id = userId });
        return result;
    }

    public async Task<ApiResult<bool>> Register(string username, string password, string email)
    {
        var result = await this.MakePostRequest<bool>("/api/v1/player/register", new { userName = username, password = password, email = email });
        return result;
    }

    private HttpClient CreateClient()
    {
        var client = new HttpClient();
        client.BaseAddress = new Uri(this.Url);
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.Add("Version", GameInfo.Version());
        return client;
    }

    private async Task<ApiResult<T>> MakeGetRequest<T>(string requestUri)
    {
        var endResult = new ApiResult<T>();
        try
        {
            using (var client = this.CreateClient())
            {
                var response = await client.GetAsync(requestUri);
                if (response.IsSuccessStatusCode)
                {
                    var s = await response.Content.ReadAsStringAsync();
                    endResult = JsonConvert.DeserializeObject<ApiResult<T>>(s);
                }
                else
                {
                    endResult.message = "Api error";
                    endResult.Result = default(T);
                }
                
                return endResult;
            }
        }
        catch (HttpRequestException httpRequestException)
        {
            endResult.message = "Api error";
            endResult.Result = default(T);
            Logger.Exception(httpRequestException);
            return endResult;

        }
        catch (WebException webException)
        {
            endResult.message = "Api error";
            endResult.Result = default(T);
            Logger.Exception(webException);
            return endResult;
        }
        catch (InvalidOperationException invalidOperationException)
        {
            endResult.message = "Api error";
            endResult.Result = default(T);
            Logger.Exception(invalidOperationException);
            return endResult;
        }
    }

    private async Task<ApiResult<T>> MakePostRequest<T>(string requestUri, object obj)
    {
        var endResult = new ApiResult<T>();
        try
        {
            using (var client = this.CreateClient())
            {
                var json = JsonConvert.SerializeObject(obj);
                Logger.Log(json);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(requestUri, content);
                Logger.Log(response.RequestMessage.RequestUri.ToString());
                Logger.Log(response.IsSuccessStatusCode.ToString());
                Logger.Log(response.StatusCode.ToString());
                if (response.IsSuccessStatusCode)
                {
                    var s = await response.Content.ReadAsStringAsync();
                    endResult = JsonConvert.DeserializeObject<ApiResult<T>>(s);
                }
                else
                {
                    
                    endResult.message = "Api error";
                    endResult.Result = default(T);
                }

                return endResult;
            }
        }
        catch (HttpRequestException httpRequestException)
        {
            endResult.message = "Api error";
            endResult.Result = default(T);
            Logger.Exception(httpRequestException);
            return endResult;

        }
        catch (WebException webException)
        {
            endResult.message = "Api error";
            endResult.Result = default(T);
            Logger.Exception(webException);
            return endResult;
        }
        catch (InvalidOperationException invalidOperationException)
        {
            endResult.message = "Api error";
            endResult.Result = default(T);
            Logger.Exception(invalidOperationException);
            return endResult;
        }
    }

}
