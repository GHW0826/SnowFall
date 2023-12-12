using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace SnowFall.Common.IdGenerator;

public class UUIDGenerator : IIdGenerator<string>
{
    public string GenerateId()
    {
        return Guid.NewGuid().ToString();
    }
}
