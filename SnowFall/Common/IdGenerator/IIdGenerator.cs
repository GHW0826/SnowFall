using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnowFall.Common.IdGenerator;

public interface IIdGenerator<T>
{
    T GenerateId();
}
