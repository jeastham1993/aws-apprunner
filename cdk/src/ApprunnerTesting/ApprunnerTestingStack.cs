using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Constructs;

namespace ApprunnerTesting
{
    public class ApprunnerTestingStack : Stack
    {
        internal ApprunnerTestingStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            var networkStack = new NetworkStack(this, "NetworkStack");

            var applicationStack = new ApplicationStack(this, "AppStack", new ApplicationStackProps(networkStack.Vpc, networkStack.ApplicationSecurityGroup));
        }
    }
}
