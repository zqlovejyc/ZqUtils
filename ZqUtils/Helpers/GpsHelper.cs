using System;
/****************************
* [Author] 张强
* [Date] 2018-10-15
* [Describe] GPS工具类
* **************************/
namespace ZqUtils.Helpers
{
    /// <summary>
    /// GPS工具类
    /// 参考源码：https://github.com/Senparc/Senparc.CO2NET/blob/master/src/Senparc.CO2NET/Helpers/GPS/GpsHelper.cs
    /// </summary>
    public class GpsHelper
    {
        #region 测距-方法1

        //以下算法参考：https://blog.csdn.net/xiejm2333/article/details/73297004

        /// <summary>
        /// 计算两点GPS坐标的距离（单位：米）
        /// </summary>
        /// <param name="n1">第一点的纬度坐标</param>
        /// <param name="e1">第一点的经度坐标</param>
        /// <param name="n2">第二点的纬度坐标</param>
        /// <param name="e2">第二点的经度坐标</param>
        /// <returns></returns>
        public static double Distance(double n1, double e1, double n2, double e2)
        {
            double Rad(double d)
            {
                return d * Math.PI / 180.00; //角度转换成弧度
            }
            double Lat1 = Rad(n1); // 纬度
            double Lat2 = Rad(n2);
            double a = Lat1 - Lat2;//两点纬度之差
            double b = Rad(e1) - Rad(e2); //经度之差
            double s = 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(a / 2), 2) + Math.Cos(Lat1) * Math.Cos(Lat2) * Math.Pow(Math.Sin(b / 2), 2)));//计算两点距离的公式
            s = s * 6378137.0;//弧长乘地球半径（半径为米）
            s = Math.Round(s * 10000d) / 10000d;//精确距离的数值
            return s;
        }
        #endregion

        #region 测距-方法2

        //参考：https://blog.csdn.net/xiejm2333/article/details/73297004

        /// <summary>
        /// 百度百科： 6371.393 km ：https://baike.baidu.com/item/%E5%9C%B0%E7%90%83%E5%8D%8A%E5%BE%84/1037801?fr=aladdin
        /// </summary>
        public static double EARTH_RADIUS = 6371393.0;//m 地球半径 平均值，千米 / 也有数据：6378137.0米

        /// <summary>
        /// 给定的经度1，纬度1；经度2，纬度2. 计算2个经纬度之间的距离。
        /// </summary>
        /// <param name="lat1">经度1</param>
        /// <param name="lon1">纬度1</param>
        /// <param name="lat2">经度2</param>
        /// <param name="lon2">纬度2</param>
        /// <returns>距离（公里、千米）</returns>
        public static double Distance2(double lat1, double lon1, double lat2, double lon2)
        {
            double HaverSin(double theta)
            {
                var v = Math.Sin(theta / 2);
                return v * v;
            }

            //将角度换算为弧度。
            double ConvertDegreesToRadians(double degrees)
            {
                return degrees * Math.PI / 180;
            }
            //用haversine公式计算球面两点间的距离。
            //经纬度转换成弧度
            lat1 = ConvertDegreesToRadians(lat1);
            lon1 = ConvertDegreesToRadians(lon1);
            lat2 = ConvertDegreesToRadians(lat2);
            lon2 = ConvertDegreesToRadians(lon2);

            //差值
            var vLon = Math.Abs(lon1 - lon2);
            var vLat = Math.Abs(lat1 - lat2);

            //h is the great circle distance in radians, great circle就是一个球体上的切面，它的圆心即是球心的一个周长最大的圆。
            var h = HaverSin(vLat) + Math.Cos(lat1) * Math.Cos(lat2) * HaverSin(vLon);

            var distance = 2 * EARTH_RADIUS * Math.Asin(Math.Sqrt(h));

            return distance;
        }
        #endregion

        #region 获取指定偏移距离的经纬度范围
        /// <summary>
        /// 获取指定偏移距离的经纬度范围
        /// </summary>
        /// <param name="lat">纬度</param>
        /// <param name="lng">经度</param>
        /// <param name="distance">偏移距离</param>
        /// <returns>经纬度范围</returns>
        public static (double minLat, double maxLat, double minLng, double maxLng) GetLatAndLngRange(double lat, double lng, double distance)
        {
            double dis = distance / 1000;
            double earthR = EARTH_RADIUS / 1000;//地球半径千米

            double dlng = 2 * Math.Asin(Math.Sin(dis / (2 * earthR)) / Math.Cos(lat * Math.PI / 180));//定义三角函数的输入和输出都采用弧度值
            dlng = dlng * 180 / Math.PI;//角度转为弧度  

            double dlat = dis / earthR;
            dlat = dlat * 180 / Math.PI;

            double minLat = lat - dlat;//最小维度
            double maxLat = lat + dlat;
            double minLng = lng - dlng;//最小经度
            double maxLng = lng + dlng;
            return (minLat, maxLat, minLng, maxLng);
        }
        #endregion

        #region 获取维度差
        /// <summary>
        /// 获取维度差
        /// </summary>
        /// <param name="km">千米</param>
        /// <returns></returns>
        public static double GetLatitudeDifference(double km)
        {
            return km * 1 / 111;
        }
        #endregion

        #region 获取经度差
        /// <summary>
        /// 获取经度差
        /// </summary>
        /// <param name="km">千米</param>
        /// <returns></returns>
        public static double GetLongitudeDifference(double km)
        {
            return km * 1 / 110;
        }
        #endregion
    }
}