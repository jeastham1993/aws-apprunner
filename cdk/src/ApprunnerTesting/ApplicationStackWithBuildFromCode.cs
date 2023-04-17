using Amazon.CDK.AWS.AppRunner.Alpha;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.IAM;
using Constructs;

namespace ApprunnerTesting;

using Amazon.CDK;

using Environment = System.Environment;

public class ApplicationStackWithBuildFromCode : Construct
{   
    public ApplicationStackWithBuildFromCode(Construct scope, string id, ApplicationStackProps props) : base(scope, id)
    {
        var applicationRole = new Role(this, "ApplicationRole", new RoleProps
        {
            AssumedBy =new ServicePrincipal("tasks.apprunner.amazonaws.com"),
            RoleName = "AppRunnerApplicationRole"
        });
        
        var subnets = props.Vpc.SelectSubnets(new SubnetSelection()
        {
            SubnetType = SubnetType.PUBLIC
        });
        
        var appRunnerService = new Service(this, "EcrApplicationService", new ServiceProps
        {
            Source = Source.FromGitHub(new GithubRepositoryProps
            {
                ConfigurationSource = ConfigurationSourceType.REPOSITORY,
                Connection = new GitHubConnection(Environment.GetEnvironmentVariable("GITHUB_CONNECTION_ARN")),
                RepositoryUrl = Environment.GetEnvironmentVariable("REPOSITORY_URL"),
                Branch = "main"
            }),
            AutoDeploymentsEnabled = false,
            InstanceRole = applicationRole,
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