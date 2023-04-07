module "vpc" {
  source = "terraform-aws-modules/vpc/aws"

  name = "main"
  cidr = "10.0.0.0/16"

  azs             = ["eu-west-1a", "eu-west-1b", "eu-west-1c"]
  private_subnets = ["10.0.1.0/24", "10.0.2.0/24", "10.0.3.0/24"]
  public_subnets  = ["10.0.101.0/24", "10.0.102.0/24", "10.0.103.0/24"]

  enable_nat_gateway = true
  enable_vpn_gateway = true
}

resource "aws_ecr_repository" "dotnet_sample_repo" {
  name                 = "dotnet-sample"
  image_tag_mutability = "MUTABLE"

  image_scanning_configuration {
    scan_on_push = true
  }
}

resource "aws_iam_role" "app_runner_build_role" {
  name = "apprunner-build-role"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Principal = {
          Service = "build.apprunner.amazonaws.com"
        }
        Action = "sts:AssumeRole"
      }
    ]
  })
}

resource "aws_iam_role_policy" "test_policy" {
  name = "ecr_pull_policy"
  role = aws_iam_role.app_runner_build_role.id

  # Permissions to pull images from ECR
  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Action = [
          "ecr:GetDownloadUrlForLayer",
          "ecr:BatchCheckLayerAvailability",
          "ecr:BatchGetImage",
          "ecr:DescribeImages",
          "ecr:GetAuthorizationToken"
        ]
        Resource = "*"
      }
    ]
  })
}

resource "aws_security_group" "app_sg" {
  name_prefix = "application_sg"

  ingress {
    from_port   = 0
    to_port     = 0
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
}


resource "aws_apprunner_vpc_connector" "connector" {
  vpc_connector_name = "vpc-connector"
  subnets            = module.vpc.public_subnets
  security_groups    = [aws_security_group.app_sg.id]
}

resource "aws_apprunner_service" "dotnet_apprunner" {
  service_name = "dotnet-sample-service"

  source_configuration {
    image_repository {
      image_configuration {
        port = "8080"
      }
      image_identifier      = "${aws_ecr_repository.dotnet_sample_repo.repository_url}:${var.image_tag}"
      image_repository_type = "ECR"
    }
    auto_deployments_enabled = false
    authentication_configuration {
      access_role_arn = aws_iam_role.app_runner_build_role.arn
    }
  }
  health_check_configuration {
    healthy_threshold = 3
    interval = 5
    path = "/health"
    protocol = "HTTP"
    timeout = 5
    unhealthy_threshold = 3
  }
  tags = {
    Name = "dotnet-sample-service"
  }
  instance_configuration {
    cpu = "512"
  }
}

output "ecr_repo_url" {
  value = aws_ecr_repository.dotnet_sample_repo.repository_url
}