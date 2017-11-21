using System;
using System.Collections.Generic;
using System.Text;

namespace StackExchange.Redis.WordQuery
{

    public interface IRedisExceptionHandler
    {
        void HandleException(Exception ex);
    }

}
