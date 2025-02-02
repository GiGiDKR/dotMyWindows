using System;
using System.Text.Json.Serialization;

namespace OhMyWindows.Models
{
    public class Package
    {
        public required string Name { get; set; }
        public required string Id { get; set; }
        public required string Source { get; set; }

        private string _category = "Autres";
        
        [JsonPropertyName("category")]
        public string Category 
        { 
            get => _category;
            set => _category = string.IsNullOrWhiteSpace(value) ? "Autres" : value;
        }

        [JsonIgnore]
        public bool IsInstalled { get; set; }
    }
}
