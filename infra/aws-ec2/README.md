# AWS EC2 Demo Deploy

This Terraform setup creates one EC2 instance and installs Docker for a simple ChatApp demo.

## Cost Note

AWS Free Tier eligibility depends on your account creation date, region, and usage. AWS documentation says older Free Tier accounts commonly mark `t2.micro` and `t3.micro` as Free Tier eligible, while accounts created on or after July 15, 2025 have different Free Tier benefits. Verify the EC2 launch screen and AWS Billing before applying.

This setup uses:

- One EC2 instance, default `t3.micro`
- One public IPv4 address
- One gp3 root EBS volume, default 20GB

Stop or destroy the instance when the demo is done.

## Prerequisites

- AWS CLI configured with credentials
- Terraform installed
- Existing EC2 key pair in your AWS region, or a local `.pem` plus generated `.pub` that Terraform can import
- PowerShell with `ssh` and `scp`

## Create EC2

Copy the example variables:

```powershell
Copy-Item infra\aws-ec2\terraform.tfvars.example infra\aws-ec2\terraform.tfvars
```

Edit:

```text
infra/aws-ec2/terraform.tfvars
```

Set:

```hcl
key_name = "your-existing-ec2-keypair-name"
ssh_cidr = "YOUR_PUBLIC_IP/32"
```

If the key pair does not exist in AWS yet, set:

```hcl
create_key_pair = true
public_key_path = "your-key.pub"
```

Generate a `.pub` file from a `.pem` private key:

```powershell
ssh-keygen -y -f .\infra\aws-ec2\your-key.pem > .\infra\aws-ec2\your-key.pub
```

Then run:

```powershell
cd infra\aws-ec2
terraform init
terraform apply
```

Terraform outputs `public_ip`, `frontend_url`, and `api_url`.

## Deploy App Code

From the repo root:

```powershell
.\infra\aws-ec2\deploy.ps1 -HostName <EC2_PUBLIC_IP> -KeyPath C:\path\to\your-key.pem
```

Open:

```text
Frontend: http://<EC2_PUBLIC_IP>
API:      http://<EC2_PUBLIC_IP>:5185
```

Default admin:

```text
Email: admin@chat.local
Password: Admin@123456
```

## Update Demo After Code Changes

Run the deploy script again:

```powershell
.\infra\aws-ec2\deploy.ps1 -HostName <EC2_PUBLIC_IP> -KeyPath C:\path\to\your-key.pem
```

## Destroy

```powershell
cd infra\aws-ec2
terraform destroy
```
