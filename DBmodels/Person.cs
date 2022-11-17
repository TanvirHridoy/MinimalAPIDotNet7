using System;
using System.Collections.Generic;

namespace MinimalAPI.DBModels
{
    public partial class Person
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public double? Age { get; set; }
        public DateTime DoB { get; set; }
    }
}
