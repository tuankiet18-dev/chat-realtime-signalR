variable "aws_region" {
  description = "AWS region to deploy the demo EC2 instance."
  type        = string
  default     = "ap-southeast-1"
}

variable "project_name" {
  description = "Name prefix for AWS resources."
  type        = string
  default     = "chatapp-demo"
}

variable "instance_type" {
  description = "Free-tier eligible instance type in many accounts/regions. Verify eligibility in your AWS account before applying."
  type        = string
  default     = "t3.micro"
}

variable "key_name" {
  description = "AWS EC2 key pair name used for SSH. Terraform creates/imports it when create_key_pair is true."
  type        = string
}

variable "create_key_pair" {
  description = "Create/import the AWS EC2 key pair from public_key_path."
  type        = bool
  default     = true
}

variable "public_key_path" {
  description = "Path to the public key file used when create_key_pair is true."
  type        = string
  default     = "oneclickhost-keypair-fixed.pub"
}

variable "ssh_cidr" {
  description = "CIDR allowed to SSH into the instance. Prefer your public IP as x.x.x.x/32."
  type        = string
  default     = "0.0.0.0/0"
}

variable "volume_size_gb" {
  description = "Root EBS size. Keep <= 30GB to stay near common Free Tier limits."
  type        = number
  default     = 20
}
