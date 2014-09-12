using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectDumper
{
    class Program
    {
        static void Main(string[] args)
        {
            var queue = new Queue
            {
                Id = 1,
                Name = "Queue Name",
                NullableInt = 1,
                Owners = new List<Owner>
                {
                    new Owner {OwnerId = 1, OwnerName = "Owner 1"},
                    new Owner {OwnerId = 2, OwnerName = "Owner 2"},
                    new Owner {OwnerId = 3, OwnerName = "Owner 3"},
                    new Owner {OwnerId = 4, OwnerName = "Owner 4"},
                },
                OtherOwners = new List<Owner>
                {
                    new Owner {OwnerId = 1, OwnerName = "Owner 1"},
                    new Owner {OwnerId = 2, OwnerName = "Owner 2"},
                }
            };

            var dic = new Dictionary<object, string>();

            ObjectDumper.Write(queue, dic);

            foreach (var item in dic)
            {
                Console.WriteLine("{0} - {1}", item.Key, item.Value);
            }
            Console.ReadKey();
        }


    }
}
