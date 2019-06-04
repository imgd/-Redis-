using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Test
{
    /// <summary>
    /// 缓存操作 redis实现
    /// 建议改用依赖注入来调用
    /// </summary>
    public class RedisCache
    {
        private string _connectionSettings { get; set; }
        private IDatabase _db { get; set; }

        public IEnumerable<string> AllKeys => throw new NotImplementedException();

        public Task<IEnumerable<string>> AllKeysAsync => throw new NotImplementedException();

        public RedisCache() { }
        public RedisCache(string connectionSettings)
        {
            _connectionSettings = connectionSettings;
            _db = new Lazy<IConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(_connectionSettings)).Value.GetDatabase();
        }

        //list 操作 ：https://blog.csdn.net/nangeali/article/details/81735443
        //默认不设置过期时间，持久化

        /// <summary>
        /// add
        /// </summary>
        /// <param name="key"></param>
        /// <param name="o"></param>
        /// <param name="expiration"></param>
        public void AddAsync(string key, string value, DateTimeOffset expiration)
        {
            TimeSpan timeSpan = expiration.DateTime.Subtract(DateTime.Now);
            _db.StringSet(key, value, timeSpan);
        }

        

        public async Task ListRightPushAsync(string key, string value)
        {
            await _db.ListRightPushAsync(key, value);
        }

        /// <summary>
        /// 先进先出，在列表的尾部插入元素 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void ListRightPush(string key, string value)
        {
            _db.ListRightPush(key, value);
        }

        /// <summary>
        /// 先进后出，在列表头部插入元素
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void ListLeftPush(string key, string value)
        {
            _db.ListLeftPush(key, value);
        }

        public async Task<string> ListLeftPopAsync(string key)
        {
            return await _db.ListLeftPopAsync(key);
        }

        /// <summary>
        /// 左边出栈，获取列表的第一个元素 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string ListLeftPop(string key)
        {
            return _db.ListLeftPop(key).ToString();
        }

        /// <summary>
        /// 右边出栈，获取列表的最后一个元素  
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string ListRightPop(string key)
        {
            return _db.ListRightPop(key).ToString();
        }
    }


    public class RedisHelper : RedisCache
    {
        public RedisHelper() : base("192.168.1.199:26379,password=123456,connectTimeout=5000,allowAdmin=false,defaultDatabase=9")
        {

        }
    }
}
