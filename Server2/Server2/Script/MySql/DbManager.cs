using System;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;

namespace Framework
{
    public partial class DbManager
    {
        public static MySqlConnection mysql;

        /// <summary>
        /// 连接数据库
        /// </summary>
        /// <param name="db">数据库名称</param>
        /// <param name="ip">IP</param>
        /// <param name="port">端口</param>
        /// <param name="user">用户名</param>
        /// <param name="pw">密码</param>
        /// <returns></returns>
        public static bool Connect(string db, string ip, int port, string user, string pw)
        {
            // 创建MySqlConnection对象
            mysql = new MySqlConnection();
            // 连接参数
            mysql.ConnectionString = $"Database={db};Data Source = {ip};port = {port};User Id = {user};Password = {pw}";
            // 连接
            try
            {
                mysql.Open();
                Console.WriteLine("[数据库]连接成功");
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("[数据库]连接失败：, " + e.Message);
                return false;
            }
        }

        /// <summary>
        /// 合法性检查
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static bool IsSafeString(string str)
        {
            return !Regex.IsMatch(str, @"[-|;|,|\/|\(|\)|\[|\]|\}|\{|%|@|\*|!|\']");
        }
    }
}