using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace PxWeb.Monitor
    {

        public partial class JsonTableQuery
        {

            [JsonProperty("query")]
            public Query2[] Query { get; set; }

            [JsonProperty("response")]
            public Response2 Response { get; set; }
        }

        public partial class JsonTableQuery
        {
            public class Query2
            {

                [JsonProperty("code")]
                public string Code { get; set; }

                [JsonProperty("selection")]
                public Selection2 Selection { get; set; }
            }
        }
        public partial class JsonTableQuery
        {
            public class Response2
            {

                [JsonProperty("format")]
                public string Format { get; set; }
            }
        }
        public partial class JsonTableQuery
        {
            public class Selection2
            {

                [JsonProperty("filter")]
                public string Filter { get; set; }

                [JsonProperty("values")]
                public string[] Values { get; set; }
            }
        }
    }

