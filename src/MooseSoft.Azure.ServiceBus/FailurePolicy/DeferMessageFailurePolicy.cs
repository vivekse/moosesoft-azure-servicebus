﻿using MooseSoft.Azure.ServiceBus.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace MooseSoft.Azure.ServiceBus.FailurePolicy
{
    /// <summary>
    /// This failure policy will defer the original Service Bus Message attempting to be processed.
    /// A new message will be created that is a pointer to the deferred message and sent to Service Bus in an atomic transaction.
    /// New messages sent to Service Bus will be delayed based upon the <see cref="IBackOffDelayStrategy"/> chosen.
    /// </summary>
    public class DeferMessageFailurePolicy : FailurePolicyBase
    {
        public DeferMessageFailurePolicy(
            Func<Exception, bool> canHandle, 
            int maxDeliveryCount = 10, 
            IBackOffDelayStrategy backOffDelayStrategy = null) 
            : base(canHandle, maxDeliveryCount, backOffDelayStrategy)
        {
        } 

        public override async Task HandleFailureAsync(MessageContext context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var deliveryCount = GetDeliveryCount(context.Message);
            if (deliveryCount >= MaxDeliveryCount)
            {
                await context.MessageReceiver.AbandonAsync(context.Message.SystemProperties.LockToken).ConfigureAwait(false);
                return;
            }

            var deferredPointer = context.Message.CreateDeferredLocatorMessage(
                BackOffDelayStrategy.Calculate(deliveryCount));

            var sender = context.CreateMessageSender();
            try
            {
                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    await sender.SendAsync(deferredPointer).ConfigureAwait(false);
                    await context.MessageReceiver.DeferAsync(context.Message.SystemProperties.LockToken)
                        .ConfigureAwait(false);
                    scope.Complete();
                }
            }
            finally
            {
                await sender.CloseAsync();
            }
        }
    }
}