using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace PXWeb.kompis
{

        public class Kompisresult
        {
        public Variable getVariable()
        {
           return variable; 
        }   


        [JsonProperty("variable")]
        public Variable variable { get; set; }
        [JsonProperty("tableId")]
        public string tableId { get; set; }
        [JsonProperty("sources")]
        public string[] sources { get; set; }
        [JsonProperty("formula")]
        public object formula { get; set; }
        [JsonProperty("expression")]
        public Expression expression { get; set; }
        }

        public class Variable
        {
        [JsonProperty("role")]
        public string role { get; set; }
        [JsonProperty("order")]
        public object order { get; set; }
        [JsonProperty("id")]
        public int id { get; set; }
        [JsonProperty("subRole")]
        public object subRole { get; set; }
        [JsonProperty("category")]
        public object category { get; set; }
        [JsonProperty("labelNo")]
        public string labelNo { get; set; }
        [JsonProperty("labelEn")]
        public string labelEn { get; set; }
        [JsonProperty("descriptionNo")]
        public string descriptionNo { get; set; }
        [JsonProperty("descriptionEn")]
        public string descriptionEn { get; set; }
        [JsonProperty("footnoteNo")]
        public string footnoteNo { get; set; }
        [JsonProperty("footnoteEn")]
        public string footnoteEn { get; set; }
        [JsonProperty("codeList")]
        public object codeList { get; set; }
        [JsonProperty("ownerSectionRole")]
        public string ownerSectionRole { get; set; }
        [JsonProperty("unit")]
        public string unit { get; set; }
        [JsonProperty("references")]
        public object[] references { get; set; }
        [JsonProperty("name")]
        public string name { get; set; }
        [JsonProperty("type")]
        public string type { get; set; }
        }

        public class Expression
        {
        [JsonProperty("datasetVtl")]
        public object datasetVtl { get; set; }
        [JsonProperty("parameters")]
        public Parameters parameters { get; set; }
        }

        public class Parameters
        {
        [JsonProperty("t1")]
        public T1 t1 { get; set; }
        [JsonProperty("t2")]
        public T2 t2 { get; set; }
        }

        public class T1
        {
        [JsonProperty("tableId")]
        public string tableId { get; set; }
        [JsonProperty("ids")]
        public string[] ids { get; set; }
        [JsonProperty("metrics")]
        public string[] metrics { get; set; }
        [JsonProperty("region")]
        public string region { get; set; }
        [JsonProperty("period")]
        public string period { get; set; }
        }

        public class T2
        {
        [JsonProperty("tableId")]
        public string tableId { get; set; }
        [JsonProperty("ids")]
        public string[] ids { get; set; }
        [JsonProperty("metrics")]
        public string[] metrics { get; set; }
        [JsonProperty("region")]
        public string region { get; set; }
        [JsonProperty("period")]
        public string period { get; set; }
        }

    }
