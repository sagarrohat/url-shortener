
# URL Shortener (Infrastructure as Code) 

Inspired from the famous system design interview questions, URL shortener, I decided to utilize my
learning from the course of cloud computing to write infrastructure as a code for this application. For
purpose of this course, I limited the functionality of the application to shorten the URL and keep track
of opens. I have used the terraform tool for writing infrastructure as code.


## API Reference

#### Generate

```http
  POST /generate
```

| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `Url` | `string` | **Required** |
| `Expiry` | `DateTime` | Time limited URLs that expire on a certain time |

#### Resolve

```http
  GET /r/${url}
```

| Parameter | Type     | Description                       |
| :-------- | :------- | :-------------------------------- |
| `url`      | `string` | **Required**|



## Tech Stack

**API:** .NET 6 Minimal API

**Database:** Postgres

**Cache:** Redis

**IaC:** Terraform

**Cloud Provider:** Azure


