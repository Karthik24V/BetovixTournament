using AutoMapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tournament.Business.Mapper
{
    public class AutoMapper : Profile
    {
        /// <summary>
        /// Mappe the value with the registered types
        /// </summary>
        /// <param name="configuration"></param>
        public AutoMapper(IConfiguration configuration)
        {
            
        }
    }
}
