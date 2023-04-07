using Amazon.CDK;
using Amazon.CDK.AWS.AppRunner.Alpha;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.ECR;
using Amazon.CDK.AWS.IAM;
using Constructs;

namespace ApprunnerTesting;

public record ApplicationStackProps(IVpc Vpc, ISecurityGroup AppSecurityGroup);

public class ApplicationStack : Construct
{   
    public ApplicationStack(Construct scope, string id, ApplicationStackProps props) : base(scope, id)
    {
        var applicationCodeRepository = new Repository(this, "ApplicationEcrRepo", new RepositoryProps
        {
            ImageScanOnPush = true,
            ImageTagMutability = TagMutability.IMMUTABLE,
            RepositoryName = "apprunner-ecr-repo"
        });
        
        var applicationRole = new Role(this, "ApplicationRole", new RoleProps
        {
            AssumedBy =new ServicePrincipal("tasks.apprunner.amazonaws.com"),
            RoleName = "AppRunnerApplicationRole"
        });

        var ecrAccessRole = new Role(this, "EcrAccessRole", new RoleProps()
        {
            AssumedBy = new ServicePrincipal("build.apprunner.amazonaws.com"),
            RoleName = "ECRAccessRole",
        });

        applicationCodeRepository.GrantPull(ecrAccessRole);

        var subnets = props.Vpc.SelectSubnets(new SubnetSelection()
        {
            SubnetType = SubnetType.PUBLIC
        });
        
        var appRunnerService = new Service(this, "EcrApplicationService", new ServiceProps
        {
            Source = Source.FromEcr(new EcrProps
            {
                Repository = applicationCodeRepository,
                ImageConfiguration = new ImageConfiguration()
                {
                    Port = 8080
                },
                TagOrDigest = "0.0.3",
            }),
            AutoDeploymentsEnabled = false,
            InstanceRole = applicationRole,
            AccessRole = ecrAccessRole,
            VpcConnector = new VpcConnector(this,
                "VpcConnector",
                new VpcConnectorProps
                {
                    Vpc = props.Vpc,
                    SecurityGroups = new ISecurityGroup[]
                    {
                        props.AppSecurityGroup
                    },
                    VpcSubnets = new SubnetSelection()
                    {
                        Subnets = subnets.Subnets
                    },
                }),
        });

        var appUrl = new CfnOutput(this, "AppUrl", new CfnOutputProps()
        {
            ExportName = "AppUrl",
            Value = appRunnerService.ServiceUrl,
            Description = "The URL of the AppRunner service"
        });
    }
}