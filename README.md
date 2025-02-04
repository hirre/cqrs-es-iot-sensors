# CQRS IoT Sensors
A CQRS + event sourcing solution for storing and viewing IoT sensor data.

Architecture overview:

![image](https://github.com/user-attachments/assets/57518d43-cc5e-49f6-9ea6-db9f3c68f82f)


## Setup

Install and run MongoDb & Redis, e.g.:
```
docker run --name mongodb -p 27017:27017 -d mongodb/mongodb-community-server:latest
docker run -d --name redis-stack-server -p 6379:6379 redis/redis-stack-server:latest
```

1. Run the .NET web API service, it uses Scalar UI for API visualization (development environment).
2. Try the endpoints.
