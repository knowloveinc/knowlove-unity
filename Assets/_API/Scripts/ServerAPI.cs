using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

using Newtonsoft.Json;
using System;
using System.Text;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace GameBrewStudios.Networking
{
    [System.Serializable]
    public class AuthToken
    {
        public string access_token;
        public string refresh_token;

        public static AuthToken current;

        public Dictionary<string, object> tokenData;

        public AuthToken(string access_token, string refresh_token = null)
        {
            this.access_token = access_token;
            this.refresh_token = refresh_token;
            Init();
        }
        /*
            {
	            "user": {
		            "role": "player",
		            "badges": [],
		            "_id": "5e179008dd4928424cab5155",
		            "email": "ss@gmail.com",
		            "username": "keitfh123",
		            "displayName": "Keither",
		            "createdAt": "2020-01-09T20:41:44.699Z",
		            "updatedAt": "2020-01-11T20:20:32.041Z",
		            "__v": 0,
		            "gellingAvatar": {
			            "gender": "Male",
			            "skinColor": 0,
			            "hairStyle": 0,
			            "hairColor": 0,
			            "suitStyle": 0,
			            "suitColor1": 0,
			            "suitColor2": 0,
			            "suitColor3": 0,
			            "suitColor4": 0,
			            "_id": "5e1a2e0e132d401e28d8ea4f",
			            "user": "5e179008dd4928424cab5155",
			            "createdAt": "2020-01-11T20:20:30.005Z",
			            "updatedAt": "2020-01-11T20:20:30.005Z",
			            "__v": 0
		            }
	            },
	            "iat": 1578841030,
	            "exp": 1578841630
            }
        */
        public bool isExpired()
        {
            //Debug.Log("CHecking if token is expired");
            if (tokenData != null && tokenData.ContainsKey("exp"))
            {
                DateTime expireTime = DateTimeOffset.FromUnixTimeSeconds((long)tokenData["exp"]).UtcDateTime;

                //TODO: Replace DateTime local time with a reference to server time.
                Debug.LogWarning("REPLACE DateTime.UtcNow with a reference to server's last reported time + deltaTime");

                if (expireTime < DateTime.UtcNow)
                {
                    Debug.LogError("TOKEN IS EXPIRED");
                    return true;
                }
                return false;
            }

            return false;
        }

        public void Init()
        {
            //string secret = "96dfa9df477adf37d7df456bcc385e86";
            string[] parts = UnityWebRequest.EscapeURL(access_token).Split(new char[] { '.' });
            int delta = parts[1].Length % 4;

            if (delta == 3)
                parts[1] += "=";
            if (delta == 2)
                parts[1] += "==";

            byte[] jsonDataByte = System.Convert.FromBase64String(parts[1]);
            string jsonData = System.Text.Encoding.ASCII.GetString(jsonDataByte);
            this.tokenData = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonData);
            //Debug.Log("After init");
        }
    }

    [System.Serializable]
    public enum HTTPMethod
    {
        GET = 0,
        POST = 1,
        DELETE = 2,
        PUT = 3,
        HEAD = 4,
        CREATE = 5,
        OPTIONS = 6,
        PATCH = 7
    }


    /// <summary>
    /// Construct a request to send to the server.
    /// </summary>
    [System.Serializable]
    public struct ServerRequest
    {

        public string endpoint;
        public HTTPMethod httpMethod;
        public Dictionary<string, object> payload;
        public byte[] upload;

        /// <summary>
        /// Leave this null if you don't need custom headers
        /// </summary>
        public Dictionary<string, string> extraHeaders;


        /// <summary>
        /// Query parameters to append to the end of the request URI
        /// <para>Example: If you include a dictionary with a key of "page" and a value of "42" (as a string) then the url would become "https: //mydomain.com/endpoint?page=42"</para>
        /// </summary>
        public Dictionary<string, string> queryParams;


        public static void CallAPI(string endPoint, HTTPMethod httpMethod, Dictionary<string, object> body = null, Action<ServerResponse> onComplete = null, bool useAuthToken = true)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            if (useAuthToken)
                headers.Add("Authorization", "Bearer " + AuthToken.current.access_token);

            new ServerRequest(endPoint, httpMethod, body, headers, useAuthToken: useAuthToken).Send((response) =>
             {
                 onComplete?.Invoke(response);
             });
        }

        public static void UploadFile(string endPoint, HTTPMethod httpMethod, byte[] file, Action<ServerResponse> onComplete = null, bool useAuthToken = true)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            if (useAuthToken)
                headers.Add("Authorization", "Bearer " + AuthToken.current.access_token);

            new ServerRequest(endPoint, httpMethod, file, headers, useAuthToken: useAuthToken).Send((response) =>
            {
                onComplete?.Invoke(response);
            });
        }

        

        /// <summary>
        /// A one-size-fits-all Response handler for easily retrieving the desired data type from a ServerResponse
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="response">Pass in the ServerResponse object that you want to have handled</param>
        /// <param name="key">Leave this null or "" empty if you want the handler to attempt to convert the root object to type T</param>
        /// <param name="onComplete">Passing Action with and embedded argument of type Question[] will attempt to convert the response.data object into a Question[] (or use the key provided and convert that instead)</param>
        public static void ResponseHandler<T>(ServerResponse response, string key = null, System.Action<T> onComplete = null)
        {
            if (response.hasError || string.IsNullOrEmpty(response.text) || response.data == null || response.data.Count == 0 || (key != null && !response.data.ContainsKey(key)))
            {
                Debug.LogError(string.IsNullOrEmpty(response.Error) ? "ResponseHandlerError:  " + JsonConvert.SerializeObject(response) : response.Error);
                onComplete?.Invoke(default);

                if (!string.IsNullOrEmpty(response.Error))
                {
                    try
                    {
                        Dictionary<string, object> err = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.Error);
                        ServerAPI.ServerError sError = new ServerAPI.ServerError();
                        sError.text = (string)err["error"];
                        sError.status = (HttpStatusCode)response.statusCode;
                        ServerAPI.BroadcastError(sError);
                    }
                    catch(System.Exception e)
                    {
                        Debug.LogError("Failed to invoke ServerError event.");
                        Debug.LogError(e.Message);


                        ServerAPI.ServerError sError = new ServerAPI.ServerError();
                        sError.text = response.Error;
                        sError.status = (HttpStatusCode)response.statusCode;
                        ServerAPI.BroadcastError(sError);
                    }
                }

                return;
            }
            if (onComplete != null)
            {
                //Use the full null checking form here because we dont want it to try to deserialize anything if the callback isnt even going to fire off
                T obj;
                try
                {
                    obj = JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(key == null ? response.data : response.data[key]));
                    onComplete?.Invoke(obj);
                }
                catch (JsonSerializationException e)
                {
                    //Debug.LogError("DESERIALIZATION FAILED: " + JsonConvert.SerializeObject(response));
                    Debug.LogError(e);
                    Debug.LogError(e.Message);

                    onComplete?.Invoke(default);
                }

            }
        }

        /// <summary>
        /// A "fire and forget" version of the standard response handler.
        /// </summary>
        /// <param name="response"></param>
        public static void ResponseHandler(ServerResponse response)
        {
            ResponseHandler<int>(response, null, null);
        }

        /// <summary>
        /// Create a basic GET request to the specified endpoint
        /// </summary>
        public ServerRequest(string endpoint)
        {
            this.endpoint = endpoint;
            this.httpMethod = HTTPMethod.GET;
            this.extraHeaders = null;
            this.payload = null;
            this.upload = null;
            this.queryParams = null;
        }

        public ServerRequest(string endpoint, Dictionary<string, string> queryParams)
        {
            this.endpoint = endpoint;
            this.httpMethod = HTTPMethod.GET;
            this.extraHeaders = null;
            this.payload = null;
            this.upload = null;
            this.queryParams = queryParams;
        }


        public ServerRequest(string endpoint, HTTPMethod httpMethod = HTTPMethod.GET, byte[] upload = null, Dictionary<string, string> extraHeaders = null, Dictionary<string, string> queryParams = null, bool useAuthToken = true)
        {
            this.endpoint = endpoint;
            this.httpMethod = httpMethod;
            this.payload = null;
            this.upload = upload;
            this.extraHeaders = extraHeaders != null && extraHeaders.Count == 0 ? null : extraHeaders; // Force extra headers to null if empty dictionary was supplied
            this.queryParams = queryParams != null && queryParams.Count == 0 ? null : queryParams;
            bool isNonPayloadMethod = (this.httpMethod == HTTPMethod.GET || this.httpMethod == HTTPMethod.HEAD || this.httpMethod == HTTPMethod.OPTIONS);

            if (this.payload != null && isNonPayloadMethod)
            {
                Debug.LogWarning("WARNING: Payloads should not be sent in GET, HEAD, OPTIONS, requests. Attempted to send a payload to: " + this.httpMethod.ToString() + " " + this.endpoint);
            }
        }

        public ServerRequest(string endpoint, HTTPMethod httpMethod = HTTPMethod.GET, Dictionary<string, object> payload = null, Dictionary<string, string> extraHeaders = null, Dictionary<string, string> queryParams = null, bool useAuthToken = true)
        {
            this.endpoint = endpoint;
            this.httpMethod = httpMethod;
            this.payload = payload != null && payload.Count == 0 ? null : payload; //Force payload to null if an empty dictionary was supplied
            this.upload = null;
            this.extraHeaders = extraHeaders != null && extraHeaders.Count == 0 ? null : extraHeaders; // Force extra headers to null if empty dictionary was supplied
            this.queryParams = queryParams != null && queryParams.Count == 0 ? null : queryParams;
            bool isNonPayloadMethod = (this.httpMethod == HTTPMethod.GET || this.httpMethod == HTTPMethod.HEAD || this.httpMethod == HTTPMethod.OPTIONS);

            if (this.payload != null && isNonPayloadMethod)
            {
                Debug.LogWarning("WARNING: Payloads should not be sent in GET, HEAD, OPTIONS, requests. Attempted to send a payload to: " + this.httpMethod.ToString() + " " + this.endpoint);
            }
        }


        /// <summary>
        /// Helper function, it j
        /// </summary>
        public void Send(System.Action<ServerResponse> OnServerResponse)
        {
            //Debug.Log("In Send()");
            ServerRequest thisRequest = this;

            Debug.Log("Sending Request: " + thisRequest.httpMethod.ToString() + " " + thisRequest.endpoint + " -- queryParams: " + thisRequest.queryParams?.Count);
            ServerAPI.SendRequest(thisRequest, (response) =>
            {
                //Log to the console so we can see when calls are made
                //Will look like: 
                //      GET /user -- queryParams: 2, extraHeaders: 2
                OnServerResponse?.Invoke(response);
            });
        }
    }

    /// <summary>
    /// All ServerAPI.SendRequest responses will invoke the callback using an instance of this class for easier handling in client code.
    /// </summary>
    [System.Serializable]
    public class ServerResponse
    {
        /// <summary>
        /// TRUE if http error OR server returns an error status
        /// </summary>
        public bool hasError;

        /// <summary>
        /// HTTP Status Code
        /// </summary>
        public int statusCode;

        /// <summary>
        /// Raw text response from the server
        /// <para>If hasError = true, this will contain the error message.</para>
        /// </summary>
        public string text;


        /// <summary>
        /// A hashtable version of the server response, null if errors or unable to convert
        /// </summary>
        public Dictionary<string, object> data
        {
            get
            {
                if (this.hasError) return new Dictionary<string, object>();
                return JsonConvert.DeserializeObject<Dictionary<string, object>>(this.text);
            }
        }

        public string Error;

        /// <summary>
        /// A texture downloaded in the webrequest, if applicable, otherwise this will be null.
        /// </summary>
        public Texture2D texture;
    }


    public class ServerAPI : MonoBehaviour
    {
        /// <summary>
        /// This would be something like "www.mydomain.com" or "api.mydomain.com". But you could also directly supply the IPv4 address of the server to speed the calls up a little bit by bypassing DNS Lookup
        /// </summary>
        //public static string SERVER_URL = "http://localhost:5052/api";
        public static string SERVER_URL = "http://3.12.251.242/api";


        private void Start()
        {
            //StartCoroutine(LoadConfig());

            if(SERVER_URL == "http://localhost:5052/api")
            {
                Debug.LogError("USING LOCALHOST FOR TESTING, REMEMBER TO CHANGE SERVER_URL BACK BEFORE PUBLISHING");
            }
            initialized = true;
        }


        IEnumerator LoadConfig()
        {
            Debug.LogWarning("Config file url: " + "file://" + Application.streamingAssetsPath + "/config.json");
            using (UnityWebRequest request = UnityWebRequest.Get("file://" + Application.streamingAssetsPath + "/config.json"))
            {

                yield return request.SendWebRequest();

                if (!request.isNetworkError && !request.isHttpError)
                {
                    Dictionary<string, object> configData = JsonConvert.DeserializeObject<Dictionary<string, object>>(request.downloadHandler.text);
                    if (configData.ContainsKey("apiUrl"))
                    {
                        SERVER_URL = (string)configData["apiUrl"];
                        Debug.Log("API URL SET TO: " + SERVER_URL);
                    }
                    initialized = true;

                }
            }
        }

        public static Dictionary<string, string> baseHeaders = new Dictionary<string, string>() {

            { "Accept", "application/json; charset=UTF-8" },
            { "Content-Type", "application/json; charset=UTF-8" },
            { "Access-Control-Allow-Credentials", "true" },
            { "Access-Control-Allow-Headers", "Accept, X-Access-Token, X-Application-Name, X-Request-Sent-Time" },
            { "Access-Control-Allow-Methods", "GET, POST, DELETE, PUT, OPTIONS, HEAD" },
            { "Access-Control-Allow-Origin", "*" }
        };


        //_instance is to store the instance, Instance is for referencing and auto-initializing an instance when needed, and returning _instance if its already initialized
        private static ServerAPI _instance;
        private static ServerAPI Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject obj = new GameObject("ServerAPI");
                    _instance = obj.AddComponent<ServerAPI>();
                }

                if (_instance == null) Debug.LogError("ERROR: Unable to create an instance of ServerAPI in the scene");

                return _instance;

            }
        }

        public static event System.Action<ServerError> OnError;


        public struct ServerError
        {
            public HttpStatusCode status;
            public string text;

            public override string ToString()
            {
                return JsonConvert.SerializeObject(this);
            }
        }

        public static void BroadcastError(ServerError error)
        {
            OnError?.Invoke(error);
        }


        public static void SendRequest(ServerRequest request, System.Action<ServerResponse> OnServerResponse = null)
        {
            Instance.StartCoroutine(Instance.ProcessRequest(request, OnServerResponse));
        }


        private static bool initialized = false;

        IEnumerator ProcessRequest(ServerRequest request, System.Action<ServerResponse> OnServerResponse = null)
        {
            if (!initialized)
            {
                Debug.Log("Pausing request until initialization is finished: " + request.endpoint);
                yield return new WaitUntil(() => initialized);
                Debug.Log("Resuming request: " + request.endpoint);
            }

            //CHECK THE TOKEN
            //USING A COROUTINE TO UPDATE THE TOKEN BEFORE MOVING FORWARD WITH THE REQUEST

            //Debug.Log(AuthToken.current?.isExpired());
            if (request.extraHeaders != null && request.extraHeaders.ContainsKey("Authorization") && request.extraHeaders["Authorization"].StartsWith("Bearer"))
            {
                if (AuthToken.current != null && AuthToken.current.isExpired())
                {
                    Dictionary<string, string> refreshHeaders = new Dictionary<string, string>() {
                        {"Authorization", "Bearer " + AuthToken.current.refresh_token }
                    };

                    ServerRequest checkTokenRequest = new ServerRequest("/refreshToken", HTTPMethod.GET, payload: null, refreshHeaders, null, false);
                    yield return Instance.StartCoroutine(Instance.DoSendAPIRequest(checkTokenRequest, (response) =>
                    {
                        //Handle assigning the new token here, or failing gracefully if the server rejects our request
                        Debug.Log("PUT REFRESHED TOKEN INTO THE AUTH TOKEN HERE");
                        Dictionary<string, string> tokenResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(response.text);
                        if (tokenResponse.ContainsKey("token"))
                        {
                            string token = tokenResponse["token"];
                            string refreshToken = null;
                            if (tokenResponse.ContainsKey("refreshToken"))
                            {
                                refreshToken = tokenResponse["refreshToken"];
                                Debug.LogWarning("NO REFRESH TOKEN RECEIVED DURING CALL TO /refreshToken");
                            }

                            AuthToken.current = new AuthToken(token, refreshToken);
                        }
                        else
                        {
                            Debug.LogError("Failed to renew access token");
                            SceneManager.LoadScene(0);
                            OnError?.Invoke(new ServerError() { status = HttpStatusCode.Unauthorized, text = "Request failed because your session expired." });
                        }
                    }));
                }
            }


            Instance.StartCoroutine(Instance.DoSendAPIRequest(request, OnServerResponse));
        }

        public static void DownloadTexture2D(string url, System.Action<Texture2D> OnComplete = null)
        {
            Instance.StartCoroutine(Instance.DoDownloadTexture2D(url, OnComplete));
        }

        protected IEnumerator DoDownloadTexture2D(string url, System.Action<Texture2D> OnComplete = null)
        {
            using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
            {
                
                www.SetRequestHeader("Access-Control-Allow-Headers", "Accept, X-Access-Token, X-Application-Name, X-Request-Sent-Time");
                www.SetRequestHeader("Access-Control-Allow-Credentials", "true");
                www.SetRequestHeader("Access-Control-Allow-Methods", "GET, POST, DELETE, PUT, OPTIONS, HEAD");
                www.SetRequestHeader("Access-Control-Allow-Origin", "*");

                Debug.Log("Downloading Texture: " + url);
                yield return www.SendWebRequest();

                Texture2D texture = DownloadHandlerTexture.GetContent(www);

                if (texture == null)
                {
                    Debug.LogError("Texture download failed for: " + url);
                }

                OnComplete?.Invoke(texture);
            }
        }


        protected IEnumerator DoSendAPIRequest(ServerRequest request, System.Action<ServerResponse> OnServerResponse = null)
        {
            //Always wait 1 frame before starting any request to the server to make sure the requesters code has exited the main thread.
            yield return null;


            //Build the URL that we will hit based on the specified endpoint, query params, etc
            string url = BuildURL(request.endpoint, request.queryParams);

            Debug.Log("ServerRequest URL: " + url);

            using (UnityWebRequest webRequest = CreateWebRequest(url, request))
            {
                webRequest.downloadHandler = new DownloadHandlerBuffer();

                float startTime = Time.time;
                float maxTimeOut = 5f;

                yield return webRequest.SendWebRequest();
                while (!webRequest.isDone)
                {
                    yield return null;
                    if (Time.time - startTime >= maxTimeOut)
                    {
                        Debug.LogError("ERROR: Exceeded maxTimeOut waiting for a response from " + request.httpMethod.ToString() + " " + url);
                        yield break;
                    }
                }

                if (!webRequest.isDone)
                {
                    OnServerResponse?.Invoke(new ServerResponse() { hasError = true, statusCode = 408, Error = "{\"error\": \"" + request.endpoint + " Timed out.\"}" });
                    yield break;
                }

                try
                {

                    Debug.Log("Server Response: " + request.httpMethod + " " + request.endpoint + " completed in " + (Time.time - startTime).ToString("n4") + " secs.\nResponse Text: <color=Yellow>" + webRequest.downloadHandler.text + "</color>");
                }
                catch
                {
                    Debug.LogError(request);
                    Debug.LogError(request.httpMethod);
                    Debug.LogError(request.endpoint);
                    Debug.LogError(webRequest);
                    Debug.LogError(webRequest.downloadHandler);
                    Debug.LogError(webRequest.downloadHandler.text);
                }


                ServerResponse response = new ServerResponse();
                response.statusCode = (int)webRequest.responseCode;
                if (webRequest.isHttpError || webRequest.isNetworkError || !string.IsNullOrEmpty(webRequest.error))
                {
                    response.hasError = true;
                    response.Error = webRequest.error != null ? webRequest.error + webRequest.downloadHandler.text : webRequest.downloadHandler.text;
                    ServerError sError = new ServerError();
                    sError.text = response.Error;
                    sError.status = (HttpStatusCode)response.statusCode;
                    ServerAPI.BroadcastError(sError);
                    Debug.LogError("ERROR: An error was encountered in the server response -- isHttpError=" + webRequest.isHttpError.ToString() + " isNetworkError=" + webRequest.isNetworkError + "  error message = " + webRequest.error + " URL = " + url);
                    OnServerResponse?.Invoke(response);
                    Debug.Log(JsonConvert.SerializeObject(webRequest));
                }
                else
                {
                    response.hasError = false;
                    response.text = webRequest.downloadHandler.text;
                    //Dictionary<string, object> responseData = JsonConvert.DeserializeObject<Dictionary<string, object>>(webRequest.downloadHandler.text);
                    // response.data = responseData;
                    OnServerResponse?.Invoke(response);
                }
            }
        }

        UnityWebRequest CreateWebRequest(string url, ServerRequest request)
        {
            UnityWebRequest webRequest;
            //string boundary = Encoding.UTF8.GetString(UnityWebRequest.GenerateBoundary());

            switch (request.httpMethod)
            {
                case HTTPMethod.POST:
                case HTTPMethod.PATCH:
                // Defaults are fine for PUT
                case HTTPMethod.PUT:

                    if (request.payload == null && request.upload != null)
                    {
                        //WWWForm form = new WWWForm();
                        //form.AddBinaryData("uploadedFile", request.upload);

                        //MultipartFormFileSection fileSection = new MultipartFormFileSection("uploadedFile", request.upload, "testImage.png", "image/png");
                        //MultipartFormDataSection filename = new MultipartFormDataSection("filename", "testImage.png");
                        //MultipartFormDataSection dataSection = new MultipartFormDataSection("uploadedFile", Encoding.UTF8.GetString(request.upload), "image/png");
                        //List<IMultipartFormSection> form = new List<IMultipartFormSection>();
                        //form.Add(fileSection);

                        List<IMultipartFormSection> form = new List<IMultipartFormSection>
                        {
                            new MultipartFormFileSection("uploadedFile", request.upload, "testImage.png", "image/png")
                        };

                        // generate a boundary then convert the form to byte[]
                        byte[] boundary = UnityWebRequest.GenerateBoundary();
                        byte[] formSections = UnityWebRequest.SerializeFormSections(form, boundary);
                        // my termination string consisting of CRLF--{boundary}--
                        //byte[] terminate = Encoding.UTF8.GetBytes(String.Concat("\r\n--", Encoding.UTF8.GetString(boundary), "--"));
                        // Make my complete body from the two byte arrays
                        //byte[] body = new byte[formSections.Length + terminate.Length];
                        //Buffer.BlockCopy(formSections, 0, body, 0, formSections.Length);
                        //Buffer.BlockCopy(terminate, 0, body, formSections.Length, terminate.Length);
                        // Set the content type - NO QUOTES around the boundary
                        string contentType = String.Concat("multipart/form-data; boundary=--", Encoding.UTF8.GetString(boundary));
                        // Make my request object and add the raw body. Set anything else you need here
                        webRequest = new UnityWebRequest();
                        webRequest.uri = new Uri(url);
                        UploadHandler uploader = new UploadHandlerRaw(formSections);
                        webRequest.uploadHandler = uploader;
                        webRequest.uploadHandler.contentType = contentType;

                        //webRequest.chunkedTransfer = true;
                        webRequest.useHttpContinue = false;

                        webRequest.method = "POST";//request.httpMethod.ToString();
                    }
                    else
                    {
                        string json = JsonConvert.SerializeObject(request.payload);
                        Debug.Log("REQUEST BODY = " + json);
                        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);
                        webRequest = UnityWebRequest.Put(url, bytes);
                        webRequest.method = request.httpMethod.ToString();
                        //webRequest.SetRequestHeader("X-HTTP-Method-Override", method);
                    }

                    break;

                case HTTPMethod.OPTIONS:
                case HTTPMethod.HEAD:
                case HTTPMethod.GET:
                    // Defaults are fine for GET
                    webRequest = UnityWebRequest.Get(url);
                    webRequest.method = request.httpMethod.ToString();
                    break;

                case HTTPMethod.DELETE:
                    // Defaults are fine for DELETE
                    webRequest = UnityWebRequest.Delete(url);
                    break;
                default:
                    throw new System.Exception("Invalid HTTP Method");
            }

            if (baseHeaders != null)
            {
                foreach (KeyValuePair<string, string> pair in baseHeaders)
                {
                    if (pair.Key == "Content-Type" && request.upload != null) continue;
                    
                    webRequest.SetRequestHeader(pair.Key, pair.Value);
                }
            }


            if (request.extraHeaders != null)
            {
                foreach (KeyValuePair<string, string> pair in request.extraHeaders)
                {
                    webRequest.SetRequestHeader(pair.Key, pair.Value);
                }
            }

            if (request.upload != null)
            {
                //webRequest.SetRequestHeader("Content-Type", "multipart/form-data");
                //webRequest.SetRequestHeader("Content-Disposition", "form-data; name=\"200000\" ");
                //webRequest.SetRequestHeader("Accept-Encoding", "gzip, deflate, br, identity, *");

                //webRequest.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
            }

            return webRequest;
        }

        string BuildURL(string endpoint, Dictionary<string, string> queryParams = null)
        {
            string ep = endpoint.StartsWith("/") ? endpoint.Trim() : "/" + endpoint.Trim();

            return (SERVER_URL + ep + GetQueryStringFromDictionary(queryParams)).Trim();
        }

        string GetQueryStringFromDictionary(Dictionary<string, string> queryDict)
        {
            if (queryDict == null || queryDict.Count == 0) return string.Empty;

            string query = "?";

            foreach (KeyValuePair<string, string> pair in queryDict)
            {
                if (query.Length > 1)
                    query += "&";

                query += pair.Key + "=" + pair.Value;
            }

            return query;
        }

        /// <summary>
        /// Determines whether or not the response is cacheable, based on the RFC specifications.
        /// <para><see cref="https://developer.mozilla.org/en-US/docs/Glossary/cacheable"/></para>
        /// </summary>
        /// <param name="statusCode">Pass in the status code returned in the server response</param>
        /// <returns>something</returns>
        public bool isCacheable(string statusCode)
        {
            if (!string.IsNullOrEmpty(statusCode))
            {
                switch (statusCode)
                {
                    case "200":
                    case "203":
                    case "204":
                    case "206":
                    case "300":
                    case "301":
                    case "404":
                    case "405":
                    case "410":
                    case "414":
                    case "501":
                        return true;
                    default:
                        return false;
                }
            }

            return false;
        }


    }
}

