# Instructions for Building and Running the Twitter Clone API using Docker

## Prerequisites

- Docker installed on your machine.
- Docker Compose installed (comes bundled with Docker Desktop for Windows and Mac).

## Steps

**1. Clone the GitHub Repository:**

```bash
git clone https://github.com/brian-w-worsham/brian.worsham.twitter.clone.git
cd brian.worsham.twitter.clone
cd worsham.twitter.clone.angular
```

**2. Build the Docker Image:**

Since the webapp service uses a build context, you'll need to build the image first. The image will also include the Angular front end.

```bash
docker-compose build webapp
```

**3. Prepare SQL Server:**

- Validate the **'./data'** directory contains the **'.bak'** backup file for SQL Server.
- Validate that the **'./scripts'** directory contains the **'restore.sql'** script.  This will be used to restore the database from the backup file.

**4. Run the Application:**

Use Docker Compose to start the services.

```bash
docker-compose up
```

Note: On the first run, SQL Server might take some time to initialize and then run the restore script. The Web API will wait for SQL Server to be ready before it starts.

**5. Access the Web Application:**

- You can access the backend API on **<http://localhost:5140/>** or **<https://localhost:7232>**.
- If there's a frontend interface, it might also be accessible via the same port or a different one specified in the Docker Compose file.

**6.Shutdown and Cleanup** (when done):

- To stop the services, you can use the following command:

```bash
docker-compose down
```

- To remove the built image (optional):

```bash
docker rmi bworsham/twitter_clone_api_deployment:v1
```
