output "public_ip" {
  description = "Public IP of the demo EC2 instance."
  value       = aws_instance.chatapp.public_ip
}

output "frontend_url" {
  description = "Frontend URL."
  value       = "http://${aws_instance.chatapp.public_ip}"
}

output "api_url" {
  description = "API URL."
  value       = "http://${aws_instance.chatapp.public_ip}:5185"
}

output "ssh_command" {
  description = "SSH command."
  value       = "ssh ubuntu@${aws_instance.chatapp.public_ip}"
}
