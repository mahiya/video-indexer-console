﻿using Newtonsoft.Json;

namespace Functions.Common.Models
{
    public class ListVideosApiResponse
    {
#nullable disable warnings
        [JsonProperty("results")]
        public Video[] Videos { get; set; }

        [JsonProperty("nextPage")]
        public Nextpage NextPage { get; set; }

        public class Nextpage
        {

            [JsonProperty("pageSize")]
            public int Pagesize { get; set; }

            [JsonProperty("skip")]
            public int Skip { get; set; }

            [JsonProperty("done")]
            public bool Done { get; set; }
        }
    }
}