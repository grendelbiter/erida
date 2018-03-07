﻿using Assets.Wulfram3.Scripts.InternalApis.Classes;
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
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

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

    public async Task<ApiResult<bool>> Logout(string userId, string username)
    {
        var result = await this.MakePostRequest<bool>("/api/v1/player/logout", new { Id = userId, userName = username });
        return result;
    }

    public async Task<ApiResult<bool>> Register(string username, string password, string email)
    {
        var result = await this.MakePostRequest<bool>("/api/v1/player/register", new { userName = username, password = password, email = email });
        return result;
    }

    public async Task<ApiResult<NewsPost>> LatestNewsPost()
    {
        var result = await this.MakeGetRequest<NewsPost>("/api/v1/player/latestnews");
        return result;
    }

    private HttpClient CreateClient()
    {
        ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;
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

    public bool MyRemoteCertificateValidationCallback(System.Object sender,
    X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        bool isOk = true;
        // If there are errors in the certificate chain,
        // look at each error to determine the cause.
        if (sslPolicyErrors != SslPolicyErrors.None)
        {
            for (int i = 0; i < chain.ChainStatus.Length; i++)
            {
                if (chain.ChainStatus[i].Status == X509ChainStatusFlags.RevocationStatusUnknown)
                {
                    continue;
                }
                chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
                chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
                bool chainIsValid = chain.Build((X509Certificate2)certificate);
                if (!chainIsValid)
                {
                    isOk = false;
                    break;
                }
            }
        }
        return isOk;
    }
}
