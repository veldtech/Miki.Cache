using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Miki.Cache
{
    public interface ICachePool
    {
		Task<ICacheClient> GetAsync();
    }
}
