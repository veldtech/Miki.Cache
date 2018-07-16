using System;
using System.Collections.Generic;
using System.Text;

namespace Miki.Cache
{
    public interface ICachePool
    {
		ICacheClient Get { get; }
    }
}
