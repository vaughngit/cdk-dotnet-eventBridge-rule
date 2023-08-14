using System.Text.Json;
using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.Events;
using Amazon.CDK.AWS.SQS;
using Constructs;


namespace CdkDotnetEventBridgeRule
{
    public class CdkDotnetEventBridgeRuleStack : Stack
    {
        internal CdkDotnetEventBridgeRuleStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            var vpc = new Vpc(this, "Vpc", new VpcProps
            {
                NatGateways = 0
            });

            // create the dead letter queue
            var deadLetterQueue = new Queue(this, "DeadLetterQueueName", new QueueProps
            {
                QueueName = "DeadLetterQueueName",
                DeliveryDelay = Duration.Millis(0),
                RetentionPeriod = Duration.Days(14),
                Encryption = QueueEncryption.UNENCRYPTED
            });

            //create the primary queue
            var serviceQueue = new Queue(this, "ServiceQueueName", new QueueProps
            {
                QueueName = "ServiceQueueName",
                DeliveryDelay = Duration.Millis(0),
                ReceiveMessageWaitTime = Duration.Seconds(20),
                VisibilityTimeout = Duration.Minutes(15),
                RetentionPeriod = Duration.Days(14),
                DeadLetterQueue = new DeadLetterQueue
                {
                    MaxReceiveCount = 4,
                    Queue = deadLetterQueue
                },
                Encryption = QueueEncryption.UNENCRYPTED
            });

            // The code that defines your stack goes here
            var eventBus = new EventBus(this, "Bus");

            var ruleTarget = new Amazon.CDK.AWS.Events.Targets.SqsQueue(serviceQueue);

            var detailType = new string[] { "replaceme" };
            var eventPattern = new EventPattern() { DetailType = detailType };

            var rule = new Rule(this, "EventRuleName", new RuleProps
            {
                EventBus = eventBus,
                Enabled = true,
                RuleName = "EventRuleName",
                Targets = new[] { ruleTarget },
                EventPattern = eventPattern
            });

            var myCfnRule = (Amazon.CDK.AWS.Events.CfnRule)rule.Node.DefaultChild;

            var mailchip = new System.Collections.Generic.Dictionary<string, string>() { { "prefix", "mailchimp:" } };
            var detailtype = new object[] { mailchip };
            var eventpattern = new System.Collections.Generic.Dictionary<string, object>() { { "detail-type", detailtype } };

            myCfnRule.EventPattern = eventpattern;

        }
    }
}
