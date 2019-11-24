using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.Models
{
    public class Actor
    {
        public int Id { get; set; }

        public string FullName { get; set; }
        public string Bio { get; set; }

        public virtual ICollection<Movie> Movies { get; set; }
    }
}
