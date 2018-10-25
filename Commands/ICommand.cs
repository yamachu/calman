using System.Collections.Generic;
using System.Threading.Tasks;
using CalMan.Entities;

namespace CalMan.Commands
{
    public interface IComamnd
    {
        Task<string> Run(SlackPostEntity postParameter, IEnumerable<string> args);
    }
}