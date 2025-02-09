# CQRS ES IoT Sensors
A CQRS + event sourcing solution for storing and viewing IoT sensor data.

Architecture overview:

![image](https://github.com/user-attachments/assets/dec57763-78fb-4c91-8e3c-330108cc4d84)



## Setup

Install and run MongoDb & Redis, e.g.:
```
docker run --name mongodb -p 27017:27017 -d mongodb/mongodb-community-server:latest --replSet rs0
docker run -d --name redis-stack-server -p 6379:6379 redis/redis-stack-server:latest
```

Then make sure host name is set to localhost:
```
docker exec -it mongodb mongosh
cfg = rs.conf();
cfg.members[0].host = "localhost:27017";
rs.reconfig(cfg, { force: true });
```

1. Run the .NET web API service, it uses Scalar UI for API visualization (development environment).
2. Try the endpoints with e.g. the test data in "TestData" folder.

## Future Improvements
For even better scalability the read model worker can be put into a separate service that listens to incoming events from a queue (e.g. RabbitMQ & Kafka). In this scenario the CQRS service publishes events to the queue which is then processed by the standalone worker service. The worker finally updates the read models.
