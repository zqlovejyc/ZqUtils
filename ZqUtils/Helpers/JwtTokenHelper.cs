#region License
/***
 * Copyright © 2018-2020, 张强 (943620963@qq.com).
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * without warranties or conditions of any kind, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
#endregion

using System;
using System.Text;
using System.Security.Claims;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
/****************************
* [Author] 张强
* [Date] 2020-03-20
* [Describe] JwtToken工具类
* * **************************/
namespace ZqUtils.Helpers
{
    /// <summary>
    /// JwtToken工具类
    /// </summary>
    public class JwtTokenHelper
    {
        /// <summary>
        /// 创建Token
        /// </summary>
        /// <param name="claims">声明集合</param>
        /// <param name="expires">过期时间，单位秒</param>
        /// <param name="secret">密钥</param>
        /// <param name="issuer">jwt签发者</param>
        /// <param name="audience">jwt接收方</param>
        /// <param name="securityAlgorithms">加密类型</param>
        /// <returns></returns>
        public static string CreateToken(IEnumerable<Claim> claims, int expires, string secret, string issuer = null, string audience = null, string securityAlgorithms = SecurityAlgorithms.HmacSha256)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var signingCredentials = new SigningCredentials(key, securityAlgorithms);
            var jwtSecurityToken = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddSeconds(expires),
                signingCredentials: signingCredentials);

            return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        }

        /// <summary>
        /// 解析Token
        /// </summary>
        /// <param name="token">JwtToken字符串</param>
        /// <param name="secret">密钥，默认null</param>
        /// <param name="validate">是否启用Token校验，默认不启用，principal返回null，若启用校验需要secret参数；</param>
        /// <returns></returns>
        public static (JwtSecurityToken securityToken, ClaimsPrincipal principal) ReadToken(string token, string secret = null, bool validate = false)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.ReadJwtToken(token);
            if (validate)
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
                var validationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    RequireExpirationTime = true,
                    IssuerSigningKey = key
                };
                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                return (securityToken, principal);
            }
            return (securityToken, null);
        }
    }
}