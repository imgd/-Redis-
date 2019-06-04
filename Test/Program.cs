using System;
using System.Threading;
using System.Threading.Tasks;

namespace Test
{
    /// <summary>
    /// Redis 队列简单实现
    /// </summary>
    class Program
    {
        //同步锁 保证 顺序
        private static object lockobj = new object();
       
        static void Main(string[] args)
        {
            Console.WriteLine($"初始化配置...");
            var helper = new RedisHelper();
            var key = "test_001";
            var thredCount = 10;
            var listCount = 2000000;
            var dtStart = DateTime.Now;

            helper.AddAsync("test_002", "21321321321",DateTime.Now.AddHours(1));

            Console.WriteLine($"开始写入数据...");
            for (int i = 1; i <= listCount; i++)
            {
                helper.ListRightPush(key, $"{i.ToString()}测试数据XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX大法师空间发挥的空间as富家大室");
                Console.WriteLine($"写入队列：{i}");
            }

            //默认执行顺序为 先进先出
            //当某个数据取出，中间业务执行异常，需要重新把它添加到第一位或优先前面的位置
            //模拟添加到第一位
            helper.ListLeftPush(key, "这个是异常插入到第一位");

            Console.WriteLine($"写入完毕，开始拉取队列数据...");

            ManualResetEvent[] _manualEvents = new ManualResetEvent[thredCount];

            for (int i = 0; i < thredCount; i++)
            {
                _manualEvents[i] = new ManualResetEvent(false);

                ThreadPool.QueueUserWorkItem((object state) =>
                {
                    var sets = state as Imput;
                    while (true)
                    {
                        string item;
                        lock (lockobj)
                        {
                            item = sets.RedisHelper.ListLeftPop(sets.Key);
                        }

                        if (string.IsNullOrEmpty(item))
                        {
                            //标识线程终止
                            sets.ManualResetEvent.Set();
                            Console.WriteLine($"线程={sets.Index} ，结束！");
                            break;
                        }

                        Console.WriteLine($"线程={sets.Index}  取出队列数据：{item}");
                    }
                }, new Imput
                {
                    Index = i,
                    Key = key,
                    RedisHelper = helper,
                    ManualResetEvent = _manualEvents[i]
                });
            }

            

            WaitHandle.WaitAll(_manualEvents);
            Console.WriteLine("任务全部结束了");
            Console.WriteLine($"执行结果：数据={listCount} 分配线程数={thredCount} 执行耗时:{(DateTime.Now - dtStart).TotalSeconds}秒。");
            Console.ReadLine();
        }

    }


    public class Imput
    {
        public int Index { get; set; }

        public string Key { get; set; }

        public RedisHelper RedisHelper { get; set; }

        public ManualResetEvent ManualResetEvent { get; set; }
    }
}
