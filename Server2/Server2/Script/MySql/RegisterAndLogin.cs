using System;
using MySql.Data.MySqlClient;

namespace Framework
{
    public partial class DbManager
    {
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
                Console.WriteLine("[数据库]语法错误：" + e.Message);
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
                Console.WriteLine("[数据库]注册失败，非法账号");
                return false;
            }

            if (!IsSafeString(pw))
            {
                Console.WriteLine("[数据库]注册失败，非法密码");
                return false;
            }

            // 能否注册
            if (!IsAccountExist(account))
            {
                Console.WriteLine("[数据库]注册失败，账号已存在");
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


        /// <summary>
        /// 账号验证
        /// </summary>
        /// <param name="account"></param>
        /// <param name="pw"></param>
        /// <returns></returns>
        public static bool CheckPassword(string account, string pw)
        {
            if (!IsSafeString(account))
            {
                Console.WriteLine("[数据库]CheckPassword Err，非法账号");
                return false;
            }

            if (!IsSafeString(pw))
            {
                Console.WriteLine("[数据库]CheckPassword Err，非法密码");
                return false;
            }

            // 查询
            var sql = $"select * from account where account = '{account}' and password = '{pw}';"; 
            try
            {
                var cmd = new MySqlCommand(sql, mysql);
                var dataReader = cmd.ExecuteReader();
                bool hasRows = dataReader.HasRows;
                dataReader.Close();
                return hasRows;
            }
            catch (Exception e)
            {
                Console.WriteLine("[数据库]CheckPassword Err：" + e.Message);
                return false;
            }
        }
    }
}