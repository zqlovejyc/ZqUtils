using System;
using System.Threading.Tasks;
/****************************
 * [Author] 张强
 * [Date] 2017-12-20
 * [Describe] 异步工具类
 * **************************/
namespace ZqUtils.Helpers
{
    /// <summary>
    /// 异步工具类
    /// </summary>
    public class TaskHelper
    {
        /// <summary>  
        /// 将一个方法function异步运行，在执行完毕时执行回调callback  
        /// </summary>  
        /// <param name="function">异步方法，该方法没有参数，返回类型必须是void</param>  
        /// <param name="callback">异步方法执行完毕时执行的回调方法，该方法没有参数，返回类型必须是void</param>  
        public static async void RunAsync(Action function, Action callback = null)
        {
            Task taskFunc() => Task.Run(() => function?.Invoke());
            await taskFunc();
            callback?.Invoke();
        }

        /// <summary>  
        /// 将一个方法function异步运行，在执行完毕时执行回调callback  
        /// </summary>  
        /// <typeparam name="T">异步方法的返回类型</typeparam>  
        /// <param name="function">异步方法，该方法没有参数，返回类型必须是TResult</param>  
        /// <param name="callback">异步方法执行完毕时执行的回调方法，该方法参数为TResult，返回类型必须是void</param>  
        public static async void RunAsync<T>(Func<T> function, Action<T> callback = null)
        {
            Task<T> taskFunc() => Task.Run(() => function == null ? default(T) : function());
            var result = await taskFunc();
            callback?.Invoke(result);
        }
    }
}
