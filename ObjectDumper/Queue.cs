
using System.Collections.Generic;

namespace ObjectDumper
{
    public enum QueueType
    {
        Unknown,
        QueueTypeOne,
        QueueTypeTwo
    };

    class Queue
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public QueueType QueueType { get; set; }

        public int? NullableInt { get; set; }

        public List<Owner> Owners { get; set; }

        public List<Owner> OtherOwners { get; set; }
    }
}
