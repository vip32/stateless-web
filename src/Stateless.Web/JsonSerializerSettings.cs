namespace Stateless.Web.Application.Workflow.Web
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Serialization;

    public static class JsonSerializerSettings
    {
        public static Newtonsoft.Json.JsonSerializerSettings Create()
        {
            return new Newtonsoft.Json.JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                TypeNameHandling = TypeNameHandling.None,
                //DateParseHandling = DateParseHandling.DateTimeOffset,
                //DateFormatHandling = DateFormatHandling.IsoDateFormat,
                //DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                Converters = new List<JsonConverter>
                {
                    //new GuidConverter(),
                    new StringEnumConverter(),
                    new IsoDateTimeConverter
                    {
                        DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffffffZ" // utc, no timezone offset (+0:00)
                    }
                }
            };
        }
    }
}
