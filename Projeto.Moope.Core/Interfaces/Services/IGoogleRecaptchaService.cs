using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projeto.Moope.Core.Interfaces.Services
{
    public interface IGoogleRecaptchaService
    {
        Task<bool> VerifyTokenAsync(string token);
    }
}
