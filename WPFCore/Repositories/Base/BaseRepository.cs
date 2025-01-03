using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Personal.Domain.Entities.Base;
using Personal.Services.Response;
using Personal.WPFClient.Helper.Window;
using WPFClient.Configuration;

namespace Personal.WPFClient.Repositories.Base;

public class BaseRepository<T> : IBaseRepository<T>, IDisposable 
{
    private readonly HttpClient _httpClient;
    private readonly ServiceConfigurationBuilder myServiceConfig;
    public BaseRepository(ServiceConfigurationBuilder serviceConfig)
    {
        myServiceConfig = serviceConfig;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue
            ("application/json"));
    }

    public string Endpoint { set; get; }
    
    public void Dispose()
    {
        _httpClient?.Dispose();
    }

    public async Task<T> CreateAsync(T item)
    {
        T ret = item;
        if (item is null) return ret;
        try
        {
            var response =
                await _httpClient.PostAsJsonAsync($"{myServiceConfig.Config.GetEndpoint(Endpoint)}/", item);

            var apiResponse = await response.Content.ReadAsStringAsync();
            var resp = JsonConvert.DeserializeObject<APIResponse>(apiResponse);
            if (resp.IsSuccess)
            {
                var jtok = (JToken)resp.Result;
                ret = jtok.ToObject<T>();
            }
        }
        catch (Exception jex)
        {
            WindowManager.ShowError(jex);
            
        }
        return ret;
    }

    public Task<IEnumerable<T>> CreateManyAsync(IEnumerable<T> items)
    {
        throw new NotImplementedException();
    }

    public async Task<T> UpdateAsync(T item)
    {
        T ret = default(T);
        try
        {
            var response =
                await _httpClient.PutAsJsonAsync($"{myServiceConfig.Config.GetEndpoint(Endpoint)}/", item);

            var apiResponse = await response.Content.ReadAsStringAsync();
            var resp = JsonConvert.DeserializeObject<APIResponse>(apiResponse);
            if (resp.IsSuccess)
            {
                var jtok = (JToken)resp.Result;
                ret = jtok.ToObject<T>();
            }
        }
        catch (Exception jex)
        {
            WindowManager.ShowError(jex);
            
        }
        return ret;
    }

    public Task<IEnumerable<T>> UpdateManyAsync(IEnumerable<T> items)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> DeleteAsync(T item)
    {
        bool ret = false;
        try
        {
            var response =
                await _httpClient.DeleteAsync($"{myServiceConfig.Config.GetEndpoint(Endpoint)}/{((IIdentity)item)._id}");

            var apiResponse = await response.Content.ReadAsStringAsync();
            var resp = JsonConvert.DeserializeObject<APIResponse>(apiResponse);
            if (resp.IsSuccess)
            {
                ret = (bool)(resp.Result ?? true);
            }
        }
        catch (Exception jex)
        {
            WindowManager.ShowError(jex);
            
        }
        return ret;
    }

    public Task<bool> DeleteManyAsync(IEnumerable<T> items)
    {
        throw new NotImplementedException();
    }

    public async Task<T> GetByIdAsync(Guid id)
    {
        try
        {
            using (var response =
                   await _httpClient.GetAsync($"{myServiceConfig.Config.GetEndpoint(Endpoint)}/{id}"))
            {
                var apiResponse = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(apiResponse)) return default;
                var resp = JsonConvert.DeserializeObject<APIResponse>(apiResponse);
                if (resp.IsSuccess)
                {
                    var jtok = (JToken)resp.Result;
                    var obj = jtok.ToObject<T>();
                    return obj;
                }
            }
        }
        catch (Exception jex)
        {
            WindowManager.ShowError(jex);
        }
        return default;
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        try
        {
            using (var response =
                   await _httpClient.GetAsync($"{myServiceConfig.Config.GetEndpoint(Endpoint)}/all"))
            {
                var apiResponse = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(apiResponse)) return new List<T>();
                var resp = JsonConvert.DeserializeObject<APIResponse>(apiResponse);
                if (resp.IsSuccess)
                {
                    var jtok = (JToken)resp.Result;
                    var obj = jtok.ToObject<List<T>>();
                    return obj;
                }
            }
        }
        catch (Exception jex)
        {
            WindowManager.ShowError(jex);
        }
        return null;
    }
}
