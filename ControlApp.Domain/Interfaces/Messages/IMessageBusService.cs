using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ControlApp.Domain.Dtos.Messaging;

namespace ControlApp.Domain.Interfaces.Messages
{
    public interface IMessageBusService
    {
        void PublishWelcomeMessage(WelcomeMessage welcomeMessage);
    }
}
