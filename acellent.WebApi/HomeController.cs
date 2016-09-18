using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using System;

namespace acellent.WebApi
{    /// <summary>
     /// 包含 People 結構定義的類別及其 Get() 方法。
     /// </summary>
    public class HomeController: ApiController
    {
        /// <summary>
        /// 公用的 People 結構定義類別及其 Get() 方法。
        /// </summary>
        public class People
        {
            private string _title = "acellent Web Api (Created by Acellent IT Studio, Tom Liao)";
            private string _time = "Executing time:" + DateTime.Now;
            private string _user = "Test User";

            /// <summary>
            /// 欄位：讀取(Get)或設定(Set)使用者姓名。註：此為 JSON 格式欄位。
            /// </summary>
            [JsonProperty]
            public string User
            {
                get { return _user; }
                set { _user = value; }
            }
            /// <summary>
            /// 欄位：讀取(Get)或設定(Set)使用者職稱。註：此為 JSON 格式欄位。
            /// </summary>
            [JsonProperty]
            public string Title
            {
                get { return _title; }
                set { _title = value; }
            }
            /// <summary>
            /// 欄位：讀取(Get)或設定(Set)資料存取時間。註：此為 JSON 格式欄位。
            /// </summary>
            [JsonProperty]
            public string ExeTime
            {
                get { return _time; }
                set { _time = value; }
            }
        }

        /// <summary>
        /// 方法：自 People 類別擷取資料。
        /// </summary>
        public People Get()
        {
            People test = new People();
            return test;
        }
    }
}
