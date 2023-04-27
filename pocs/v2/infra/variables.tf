variable "project" {
  type        = string
  description = "Project name for resource tagging"
  default     = "tcc"
}

variable "organization" {
  type        = string
  description = "Organization that owns or suppoert the project"
  default     = "mack"
}

variable "aws_region" {
  type        = string
  description = "AWS region to use for resources"
  default     = "us-east-1"
}

variable "vpc_cidr_block" {
  type        = string
  description = "Base CIDR Block for VPC"
  default     = "10.0.0.0/16"
}

variable "vpc_subnets_cidr_block" {
  type        = list(string)
  description = "CIDR Blocks for subnets in VPC"
  default     = ["10.0.0.0/24", "10.0.1.0/24"]
}

variable "broker_ec2_instance_type" {
  type        = string
  description = "Type for Broker EC2 instance"
  default     = "t2.micro"
}