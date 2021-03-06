﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WeChat.Model;

namespace WeChat
{

    /// <summary>
    /// 该类提供AccessToken的管理，请在实际使用过程中保持该类为单例对象
    /// </summary>
    public class AccessTokenManager : IDisposable
    {

        HttpClient _client;
        Timer getTokenTimer;
        readonly string appId;
        readonly string secret;
        public AccessTokenManager(HttpClient client, string appId, string secret)
        {
            this._client = client;
            this.appId = appId;
            this.secret = secret;
        }

        public void Dispose()
        {
            getTokenTimer.Dispose();
        }

        /// <summary>
        /// 获取令牌
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="secret"></param>
        /// <returns></returns>
        public async Task<GetAccessTokenResponse> GetToken()
        {
            // {"access_token":"35_Kfc59VZI3rio3gSU6R_m0WKiq0EtILksW5igwkT4s0sYKGttSjtABEjewV-Aj0ZIic7ijDjrOIqKxKY_6KjOp5lc6Wn1NPJJ5vXtGNarJivhf8IIy4PilEl1cV0BxCnl1enzBU_0AZPr4fQBQDFjACAVIK","expires_in":7200}

            string url = $"https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={appId}&secret={secret}";
            GetAccessTokenResponse response = await this._client.GetAsync<GetAccessTokenResponse>(url);
            return response;
        }

        public async Task<GetTicketResponse> GetTicket(string accessToken)
        {
            string url = $"https://api.weixin.qq.com/cgi-bin/ticket/getticket?access_token={accessToken}&type=jsapi";
            GetTicketResponse response = await this._client.GetAsync<GetTicketResponse>(url);
            return response;
        }


        /// <summary>
        /// 开启定时获取AccessToken，如果开启该功能，请确保当前实例为单例
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public void StartTimingGetToken(Action<StartTimingGetTokenOutput> excute,int? second=null)
        { 
            int period = (second ?? 7200) * 1000;
            this.getTokenTimer = new Timer(async (obj) =>
            {
                try
                {

                    var token = await GetToken();
                    if (token == null)
                    {
                        throw new Exception("获取不到accessToken");
                    }
                    var ticket = await GetTicket(token.access_token);
                    if (ticket == null)
                    {
                        throw new Exception("获取不到jsapiticket");
                    }
                    excute(new StartTimingGetTokenOutput()
                    {
                        AccessToken = token.access_token,
                        JSAPITicket = ticket.ticket
                    });
                }
                catch (Exception ex) {
                    Console.WriteLine(ex);
                }

            }, null, 0, period);
        }

    }
}
