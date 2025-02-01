using System;

namespace OhMyWindows.Models
{
    public class Package
    {
        public required string Name { get; set; }
        public required string Id { get; set; }
        public required string Source { get; set; }
        public required string Category { get; set; }
    }
}
