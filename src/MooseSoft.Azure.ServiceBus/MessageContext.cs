﻿using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using System;

namespace MooseSoft.Azure.ServiceBus
{
    /// <summary>
    /// 
    /// </summary>
    public class MessageContext
    {
        /// <summary>
        /// 
        /// </summary>
        public Message Message { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IMessageReceiver MessageReceiver { get; }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IMessageSender CreateMessageSender() => new MessageSender(MessageReceiver.ServiceBusConnection, MessageReceiver.Path);
        /// <summary>
        /// /
        /// </summary>
        /// <param name="message"></param>
        /// <param name="messageReceiver"></param>
        public MessageContext(Message message, IMessageReceiver messageReceiver)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
            MessageReceiver = messageReceiver ?? throw new ArgumentNullException(nameof(messageReceiver));
        }
    }
}
