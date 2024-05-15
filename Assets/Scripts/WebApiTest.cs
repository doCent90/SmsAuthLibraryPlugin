using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class WebApiTest : MonoBehaviour
{
    [SerializeField] private string _host;
    [SerializeField] private TodoItem _item;

    [SerializeField] private Button _post;

    private HttpClient _httpClient;

    private void Start()
    {
        _httpClient = new HttpClient();

        _post.onClick.AddListener(() => Post());
    }

    private async void Post()
    {
        var result = await PostAsync();

        Debug.Log("Post with result: " + result);
    }

    private async Task<HttpStatusCode> PostAsync()
    {
        TodoItem todoItem = _item;

        var json = JsonConvert.SerializeObject(todoItem);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"https://localhost:{_host}/api/TodoItems", content);

        Debug.Log(response.StatusCode);
        response.EnsureSuccessStatusCode();

        return response.StatusCode;
    }
}

[Serializable]
public class TodoItem
{
    public long Id;
    public string Name;
    public bool IsComplete;
}
