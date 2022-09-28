variable "postgresql_server_user" {
  type    = string
  default = "postgres"
}

variable "postgresql_server_password" {

  type    = string
  default = "SnH5x454nn4Iaaxp"
}

variable "postgresql_database_name" {
  type    = string
  default = "systemdb"
}

variable "api_instances_count" {
  type    = number
  default = 5
}
