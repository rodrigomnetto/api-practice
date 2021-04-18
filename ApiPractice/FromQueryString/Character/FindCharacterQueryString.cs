﻿using Newtonsoft.Json;

namespace ApiPractice.FromQueryString.Character
{
    public class FindCharacterQueryString
    {
        private int _take;

        [JsonProperty("skip")]
        public int Skip { get; set; }

        [JsonProperty("take")]
        public int Take { get => _take == 0 ? 100 : _take; set => _take = value; }

        [JsonProperty("nameStartsWith")]
        public string NameStartsWith { get; set; }
    }
}
