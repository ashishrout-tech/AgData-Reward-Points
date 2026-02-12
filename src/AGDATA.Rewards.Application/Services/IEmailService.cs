using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Application.Services
{
    public interface IEmailService
    {
        Task<bool> SendAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default);
    }
}
