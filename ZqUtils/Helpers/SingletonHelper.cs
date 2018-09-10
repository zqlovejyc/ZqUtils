/****************************
* [Author] 张强
* [Date] 2015-10-26
* [Describe] 泛型单例工具类
* **************************/
namespace ZqUtils.Helpers
{
    /// <summary>
    /// 泛型单例工具类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SingletonHelper<T> where T : class, new()
    {
        #region 私有字段
        /// <summary>
        /// 静态私有对象
        /// </summary>
        private static T _instance;
        
        /// <summary>
        /// 线程对象，线程锁使用
        /// </summary>
        private static readonly object locker = new object();
        #endregion

        #region 公有方法
        /// <summary>
        /// 静态获取实例方法
        /// </summary>
        /// <returns>T</returns>
        public static T GetInstance()
        {
            if (_instance == null)
            {
                lock (locker)
                {
                    if (_instance == null) _instance = new T();
                }
            }
            return _instance;
        }
        #endregion
    }
}
