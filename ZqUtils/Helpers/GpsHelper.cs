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
        /// <summary>
        /// 计算两点GPS坐标的距离（单位：米）
        /// 参考：https://blog.csdn.net/xiejm2333/article/details/73297004
        /// </summary>
        /// <param name="lat1">纬度1</param>
        /// <param name="lng1">经度1</param>
        /// <param name="lat2">纬度2</param>
        /// <param name="lng2">经度2</param>
        /// <param name="earthRadius">地球半径，默认：6378137.0米</param>
        /// <returns>距离（单位：米）</returns>
        public static double Distance(double lat1, double lng1, double lat2, double lng2, double earthRadius = 6378137.0)
        {
            double Rad(double d)
            {
                return d * Math.PI / 180.00; //角度转换成弧度
            }
            double Lat1 = Rad(lat1); // 纬度
            double Lat2 = Rad(lat2);
            double a = Lat1 - Lat2;//两点纬度之差
            double b = Rad(lng1) - Rad(lng2); //经度之差
            double s = 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(a / 2), 2) + Math.Cos(Lat1) * Math.Cos(Lat2) * Math.Pow(Math.Sin(b / 2), 2)));//计算两点距离的公式
            var distance = s * earthRadius;//弧长乘地球半径（半径为米）
            return distance;
        }
        #endregion

        #region 测距-方法2
        /// <summary>
        /// 计算两点GPS坐标的距离（单位：米）
        /// 参考：https://www.cnblogs.com/softfair/p/distance_of_two_latitude_and_longitude_points.html
        /// </summary>
        /// <param name="lat1">纬度1</param>
        /// <param name="lng1">经度1</param>
        /// <param name="lat2">纬度2</param>
        /// <param name="lng2">经度2</param>
        /// <param name="earthRadius">地球半径，默认：6371393.0米</param>
        /// <returns>距离（单位：米）</returns>
        public static double Distance2(double lat1, double lng1, double lat2, double lng2, double earthRadius = 6371393.0)
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
            lng1 = ConvertDegreesToRadians(lng1);
            lat2 = ConvertDegreesToRadians(lat2);
            lng2 = ConvertDegreesToRadians(lng2);

            //差值
            var vLon = Math.Abs(lng1 - lng2);
            var vLat = Math.Abs(lat1 - lat2);

            //h is the great circle distance in radians, great circle就是一个球体上的切面，它的圆心即是球心的一个周长最大的圆。
            var h = HaverSin(vLat) + Math.Cos(lat1) * Math.Cos(lat2) * HaverSin(vLon);
            var distance = 2 * earthRadius * Math.Asin(Math.Sqrt(h));

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
        /// <param name="earthRadius">地球半径，默认：6371393.0米</param>
        /// <returns>经纬度范围</returns>
        public static (double minLat, double maxLat, double minLng, double maxLng) GetLatAndLngRange(double lat, double lng, double distance, double earthRadius = 6371393.0)
        {
            double dis = distance / 1000;
            double earthR = earthRadius / 1000;//地球半径千米

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
    }
}