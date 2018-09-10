using System;
/****************************
* [Author] 张强
* [Date] 2018-05-15
* [Describe] Int扩展类
* **************************/
namespace ZqUtils.Extensions
{
    /// <summary>
    /// Int扩展类
    /// </summary>
    public static partial class Extensions
    {
        #region BuildRandCode
        /// <summary>
        /// 创建随机字符串
        /// </summary>
        /// <param name="this">字符串长度</param>
        /// <returns>string</returns>
        public static string BuildRandCode(this int @this)
        {
            var codeSerial = "0,1,2,3,4,5,6,7,8,9,a,b,c,d,e,f,g,h,i,j,k,l,m,n,o,p,q,r,s,t,u,v,w,x,y,z,A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z";
            if (@this == 0)
            {
                @this = 16;
            }
            var arr = codeSerial.Split(',');
            var code = "";
            var randValue = -1;
            var rand = new Random(unchecked((int)DateTime.Now.Ticks));
            for (var i = 0; i < @this; i++)
            {
                randValue = rand.Next(0, arr.Length - 1);
                code += arr[randValue];
            }
            return code;
        }
        #endregion
    }
}
