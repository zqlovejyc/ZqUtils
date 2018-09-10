using System;
using System.Web;
using System.Collections.Generic;
using System.Web.Caching;
/****************************
 * [Author] 张强
 * [Date] 2016-04-26
 * [Describe] 缓存工具类
 * **************************/
namespace ZqUtils.Helpers
{
    /// <summary>
    /// Cache工具类
    /// </summary>
    public class CacheHelper
    {
        #region 构造函数
        /// <summary>
        /// 私有字段
        /// </summary>
        private static Cache _cache;
        
        /// <summary>
        /// 静态构造函数
        /// </summary>
        static CacheHelper()
        {
            _cache = HttpRuntime.Cache;
        }
        #endregion

        #region 获取缓存
        /// <summary>
        /// 获取指定键的值
        /// </summary>
        /// <param name="key">要检索的缓存项的标识符</param>
        /// <returns>object</returns>
        public static object Get(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }
            return _cache.Get(key);
        }
        
        /// <summary>
        /// 获取指定键和类型的值
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="key">要检索的缓存项的标识符</param>
        /// <returns>T</returns>
        public static T Get<T>(string key)
        {
            var obj = Get(key);
            return obj == null ? default(T) : (T)obj;
        }
        
        /// <summary>
        /// 获取所有键
        /// </summary>
        /// <returns>所有键的集合</returns>
        public static IList<string> GetKeys()
        {
            var keys = new List<string>();
            var enumerator = _cache.GetEnumerator();
            while (enumerator.MoveNext())
            {
                keys.Add(enumerator.Key.ToString());
            }
            return keys.AsReadOnly();
        }
        #endregion

        #region 插入缓存
        /// <summary>
        /// 插入缓存
        /// </summary>
        /// <param name="key">用于引用项的缓存密钥</param>
        /// <param name="value">要插入到缓存的对象</param>
        public static void Insert(string key, object value)
        {
            _cache.Insert(key, value);
        }
        
        /// <summary>
        /// 插入缓存
        /// </summary>
        /// <param name="key">用于引用项的缓存密钥</param>
        /// <param name="value">要插入到缓存的对象</param>
        /// <param name="seconds">超过多少秒不调用就失效/超过多少秒就过期</param>
        /// <param name="isAbsoluteExpiration">是否是绝对过期时间，默认：否</param>
        public static void Insert(string key, object value, long seconds, bool isAbsoluteExpiration = false)
        {
            if (isAbsoluteExpiration)
            {
                _cache.Insert(key, value, null, DateTime.Now.AddSeconds(seconds), TimeSpan.Zero);
            }
            else
            {
                _cache.Insert(key, value, null, DateTime.MaxValue, TimeSpan.FromSeconds(seconds));
            }
        }
        
        /// <summary>
        /// 插入缓存
        /// </summary>
        /// <param name="key">用于引用项的缓存密钥</param>
        /// <param name="value">要插入到缓存的对象</param>
        /// <param name="dependencies">文件或缓存关键的依存关系所插入对象。 当任何依赖关系更改时，该对象将变为无效，并从缓存中删除。 如果没有依赖关系，此参数包含 null</param>
        public static void Insert(string key, object value, CacheDependency dependencies)
        {
            _cache.Insert(key, value, dependencies);
        }
        
        /// <summary>
        /// 插入缓存
        /// </summary>
        /// <param name="key">用于引用该对象的缓存密钥</param>
        /// <param name="value">要插入到缓存的对象</param>
        /// <param name="dependencies">文件或缓存关键的依存关系所插入对象。 当任何依赖关系更改时，该对象将变为无效，并从缓存中删除。 如果没有依赖关系，此参数包含 null</param>
        /// <param name="absoluteExpiration">从该处插入的对象过期并从缓存中删除的时间。 若要避免的本地时间，如从标准时间到夏时制的更改可能存在的问题，请使用 System.DateTime.UtcNow，而不是 System.DateTime.Now 为此参数值。 如果您使用的绝对过期 slidingExpiration 参数必须是 System.Web.Caching.Cache.NoSlidingExpiration</param>
        /// <param name="slidingExpiration">对象的到期时间和上次访问所插入的对象的时间之间的间隔。 如果此值为 20 分钟的等效项，该对象会过期，可从缓存中删除上次访问后的 20 分钟。 如果您使用可调到期，absoluteExpiration 参数必须是 System.Web.Caching.Cache.NoAbsoluteExpiration</param>
        public static void Insert(string key, object value, CacheDependency dependencies, DateTime absoluteExpiration, TimeSpan slidingExpiration)
        {
            _cache.Insert(key, value, dependencies, absoluteExpiration, slidingExpiration);
        }
        
        /// <summary>
        /// 插入缓存
        /// </summary>
        /// <param name="key">用于引用该对象的缓存密钥</param>
        /// <param name="value">要插入到缓存的对象</param>
        /// <param name="dependencies">文件或缓存关键的依存关系所插入对象。 当任何依赖关系更改时，该对象将变为无效，并从缓存中删除。 如果没有依赖关系，此参数包含 null</param>
        /// <param name="absoluteExpiration">从该处插入的对象过期并从缓存中删除的时间。 若要避免的本地时间，如从标准时间到夏时制的更改可能存在的问题，请使用 System.DateTime.UtcNow，而不是 System.DateTime.Now 为此参数值。 如果您使用的绝对过期 slidingExpiration 参数必须是 System.Web.Caching.Cache.NoSlidingExpiration</param>
        /// <param name="slidingExpiration">对象的到期时间和上次访问所插入的对象的时间之间的间隔。 如果此值为 20 分钟的等效项，该对象会过期，可从缓存中删除上次访问后的 20 分钟。 如果您使用可调到期，absoluteExpiration 参数必须是 System.Web.Caching.Cache.NoAbsoluteExpiration</param>
        /// <param name="onUpdateCallback">从缓存中删除该对象之前将调用一个委托。 您可以用于更新缓存的项目，并确保它不删除从缓存</param>
        public static void Insert(string key, object value, CacheDependency dependencies, DateTime absoluteExpiration, TimeSpan slidingExpiration, CacheItemUpdateCallback onUpdateCallback)
        {
            _cache.Insert(key, value, dependencies, absoluteExpiration, slidingExpiration, onUpdateCallback);
        }
        
        /// <summary>
        /// 插入缓存
        /// </summary>
        /// <param name="key">用于引用该对象的缓存密钥</param>
        /// <param name="value">要插入到缓存的对象</param>
        /// <param name="dependencies">文件或缓存关键的依存关系所插入对象。 当任何依赖关系更改时，该对象将变为无效，并从缓存中删除。 如果没有依赖关系，此参数包含 null</param>
        /// <param name="absoluteExpiration">从该处插入的对象过期并从缓存中删除的时间。 若要避免的本地时间，如从标准时间到夏时制的更改可能存在的问题，请使用 System.DateTime.UtcNow，而不是 System.DateTime.Now 为此参数值。 如果您使用的绝对过期 slidingExpiration 参数必须是 System.Web.Caching.Cache.NoSlidingExpiration</param>
        /// <param name="slidingExpiration">对象的到期时间和上次访问所插入的对象的时间之间的间隔。 如果此值为 20 分钟的等效项，该对象会过期，可从缓存中删除上次访问后的 20 分钟。 如果您使用可调到期，absoluteExpiration 参数必须是 System.Web.Caching.Cache.NoAbsoluteExpiration</param>
        /// <param name="priority">与存储在缓存中，如通过所表示的其他项相关对象的成本 System.Web.Caching.CacheItemPriority 枚举。 逐出对象; 时，缓存使用此值具有较低的成本的对象会从缓存后再成本较高的对象</param>
        /// <param name="onRemoveCallback">从缓存中删除该对象之前将调用一个委托。 您可以用于更新缓存的项目，并确保它不删除从缓存</param>
        public static void Insert(string key, object value, CacheDependency dependencies, DateTime absoluteExpiration, TimeSpan slidingExpiration, CacheItemPriority priority, CacheItemRemovedCallback onRemoveCallback)
        {
            _cache.Insert(key, value, dependencies, absoluteExpiration, slidingExpiration, priority, onRemoveCallback);
        }
        #endregion

        #region 移除缓存
        /// <summary>
        /// 移除缓存
        /// </summary>
        /// <param name="key">要移除的缓存项的标识符</param>
        public static void Remove(string key)
        {
            if (string.IsNullOrEmpty(key)) return;
            _cache.Remove(key);
        }
        
        /// <summary>
        /// 移除所有缓存
        /// </summary>
        public static void RemoveAll()
        {
            var keys = GetKeys();
            foreach (var key in keys)
            {
                _cache.Remove(key);
            }
        }
        #endregion
    }
}
