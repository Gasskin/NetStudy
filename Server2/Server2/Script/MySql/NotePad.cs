using System;
using MySql.Data.MySqlClient;

namespace Framework
{
    public partial class  DbManager
    {
        public static void GetNotePad(MsgGetText msg)
        {
            msg.text = "";
            // 防SQL注入
            if (!IsSafeString(msg.account))
            {
                msg.result = 1;
                return;
            }
            // 查询
            try
            {
                var cmd = mysql.CreateCommand();
                cmd.CommandText = $"select * from notepad where account = '{msg.account}';";
                using (var read = cmd.ExecuteReader())
                {
                    if (!read.HasRows)
                    {
                        msg.result = 2;
                    }
                    else
                    {
                        read.Read();
                        msg.result = 0;
                        msg.text = read.GetString("text");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[数据库]语法错误：" + e.Message);
            }
        }

        public static void SaveNotePad(MsgSaveText msg)
        {
            // 防SQL注入
            if (!IsSafeString(msg.account))
            {
                msg.result = 1;
                return;
            }
            
            // 查询
            try
            {
                var cmd = new MySqlCommand($"select * from notepad where account = '{msg.account}';", mysql);
                var has = false;
                using (var read = cmd.ExecuteReader()) 
                    has = read.HasRows;
                
                // 不存在那么新增
                if (!has)
                {
                    var add = new MySqlCommand($"insert into notepad set account = '{msg.account}',text ='{msg.text}';", mysql);
                    add.ExecuteNonQuery();
                }
                // 存在那么修改
                else
                {
                    var update = new MySqlCommand($"update notepad set text = '{msg.text}' where account = '{msg.account}';", mysql);
                    update.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[数据库]语法错误：" + e.Message);
            }
        }
    }
}