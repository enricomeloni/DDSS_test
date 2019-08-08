using System.Collections.Generic;
using System.Threading;

namespace CurlieIndex
{
    public class Category
    {
        private static int LastId = 1;

        public int Id { get; }
        public string Name { get; set; }
        public string Url { get; set; }

        //todo: can have multiple parents
        public Category Parent { get; set; }
        public List<int> Related { get; set; }
        public List<int> OtherLanguages { get; set; }

        public Category()
        {
            Id = LastId;
            Interlocked.Increment(ref LastId);
        }
    }
}