# cqrs-iot

Install MongoDb & Redis, e.g.:
```
docker pull mongodb/mongodb-community-server:latest
docker run --name mongodb -p 27017:27017 -d mongodb/mongodb-community-server:latest
docker run -d --name redis-stack-server -p 6379:6379 redis/redis-stack-server:latest
```