variable "image_tag" {
  type = string
}

variable "ecr_repository_url" {
  type = string
}

variable "vpc_id" {
  type = string
}

variable "public_subnets" {
  type = list(string)
}