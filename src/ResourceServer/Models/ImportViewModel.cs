using System.Collections.Generic;
using ResourceServer.Data;
using ResourceServer.Services;

namespace ResourceServer.Models
{
    public class ImportViewModel
    {
        public IEnumerable<UserResult> NewUsers { get; set; }

        public IEnumerable<Teacher> Teachers { get; set; }

        public IEnumerable<Student> Students { get; set; }
    }
}
