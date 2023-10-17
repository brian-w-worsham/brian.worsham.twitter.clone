# brian.worsham.twitter.clone
This is a clone of the Twitter website.  The project fulfills a requirement of the Simplilearn .NET Full Stack Developer Course.

## Twitter Clone API Deployment Guide
This guide will walk you through deploying the Twitter Clone API using Docker.

### Prerequisites
- Docker installed on your machine.
- Docker Compose installed (comes bundled with Docker Desktop for Windows and Mac).
- The Dockerfile for the backend.
- The docker-compose.yml file.

### Steps to Deploy
**1. Clone the GitHub Repository:**

If you have placed the docker-compose.yml in a GitHub repository, first clone the repository.

```
git clone [your-repository-url]
cd [repository-directory]
```
**2. Pull the Docker Image (Optional):**

If you've already pushed the backend image to Docker Hub, you can pull it with:
```
docker pull bworsham/twitter_clone_api_deployment:v1
```
**3. Build the Backend Docker Image (If you didn't pull from Docker Hub):**


Using the Dockerfile:

```
docker build -t bworsham/twitter_clone_api_deployment:v1 -f Dockerfile.backend .
```
**4. Run Docker Compose:**

This will start the SQL Server and the Web API services.

```
docker-compose up
```
Note: On the first run, SQL Server might take some time to initialize and then run the restore script. The Web API will wait for SQL Server to be ready before it starts.

**5. Access the Web Application:**

After starting the services with Docker Compose, you can access the web application on:

```
http://localhost:5140
```
Or
```
http://localhost:7232
```

**6. Shutdown the Application:**

When done, you can stop the services by running:
```
docker-compose down
```

### Notes and Best Practices:
- ***Passwords and Secrets:*** This demonstration uses hardcoded passwords for clarity. In a real-world scenario, never hard-code secrets in your configuration files. Use secure methods such as Docker secrets or environment variables.

- ***Persistence:*** The current setup might not persist data beyond the lifecycle of the containers. If you need persistent storage for the database, consider mounting a Docker volume to the appropriate directory in the SQL Server container.

- ***Scaling and Production:*** This setup is meant for demonstration. For production deployments, consider using orchestrators like Kubernetes, and ensure the database setup adheres to best practices for production usage.
