using System.Data;
using System.IO;
/****************************
* [Author] 张强
* [Date] 2018-05-15
* [Describe] DataSet扩展类
* **************************/
namespace ZqUtils.Extensions
{
    /// <summary>
    /// DataSet扩展类
    /// </summary>
    public static partial class Extensions
    {
        #region DataSet转Xml
        /// <summary>
        /// DataSet转Xml
        /// </summary>
        /// <param name="this">DataSet数据源</param>
        /// <returns>string</returns>
        public static string ToXml(this DataSet @this)
        {
            var result = string.Empty;
            if (@this?.Tables.Count > 0)
            {
                using (var writer = new StringWriter())
                {
                    @this.WriteXml(writer);
                    result = writer.ToString();
                }
            }
            return result;
        }
        #endregion
    }
}
