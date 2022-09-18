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
                Console.WriteLine("[数据库]connect success");
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("[数据库]connect fail, " + e.Message);
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

        /// <summary>
        /// 账号是否存在
        /// </summary>
        /// <param name="id">张海ID</param>
        /// <returns></returns>
        public static bool IsAccountExist(string id)
        {
            // 防SQL注入
            if (!IsSafeString(id))
                return false;

            // SQL语句
            string s = $"select * from account where account = '{id}';";
            
            // 查询
            try
            {
                var cmd = new MySqlCommand(s, mysql);
                var dataReader = cmd.ExecuteReader();
                var hasRows = dataReader.HasRows;
                dataReader.Close();
                return !hasRows;
            }
            catch (Exception e)
            {
                Console.WriteLine("[数据库]IsSafeString err, " + e.Message);
                return false;
            }
        }

        /// <summary>
        /// 注册账号
        /// </summary>
        /// <param name="account"></param>
        /// <param name="pw"></param>
        /// <returns></returns>
        public static bool Register(string account, string pw)
        {
            // 防SQL注入
            if (!IsSafeString(account))
            {
                Console.WriteLine("[数据库]Register fail, id not safe");
                return false;
            }

            if (!IsSafeString(pw))
            {
                Console.WriteLine("[数据库]Register fail, pw not safe");
                return false;
            }

            // 能否注册
            if (!IsAccountExist(account))
            {
                Console.WriteLine("[数据库]Register fail, id exist");
                return false;
            }

            // 写入数据库User表
            string sql = $"insert into account set account= '{account}',password= '{pw}'";
            try
            {
                MySqlCommand cmd = new MySqlCommand(sql, mysql);
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("[数据库] Register fail " + e.Message);
                return false;
            }
        }

    }
}