using System;
using System.Collections.Generic;
using System.Linq;
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
        public List<Category> Parents { get; } = new List<Category>();
        public List<int> Related { get; set; }
        public List<int> OtherLanguages { get; set; }

        public Category PrimaryParent
        {
            get
            {
                foreach (var parent in Parents)
                {
                    if (Url.StartsWith(parent.Url))
                        return parent;
                }

                return null;
            }
        }

        public Category()
        {
            Id = LastId;
            Interlocked.Increment(ref LastId);
        }
    }
}