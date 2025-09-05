# Pitstop - A Cloud-Native Microservices Demonstration

Pitstop is a comprehensive demonstration project showcasing a modern, cloud-native application built with a .NET-based microservices architecture. The application serves as a garage management system, allowing users to manage customers, vehicles, and workshop maintenance schedules.

This project is designed to be run locally using Docker Compose or deployed to a Kubernetes cluster with an Istio service mesh, all automated via a CI/CD pipeline.



## ✨ Features
- **Customer Management**: Register and view customer details.
- **Vehicle Management**: Register vehicles and assign them to owners.
- **Workshop Management**: Schedule and manage vehicle maintenance jobs on a daily calendar.
- **Email Notifications**: Automatically sends email notifications for key events.

## 💻 Technology Stack
- **Backend**: .NET, ASP.NET Core
- **Frontend**: ASP.NET Core MVC with Tailwind CSS
- **Communication**: RabbitMQ (Event Bus)
- **Data Storage**: SQL Server
- **Containerization**: Docker
- **Orchestration**: Kubernetes (Azure Kubernetes Service - AKS)
- **Service Mesh**: Istio
- **CI/CD**: GitHub Actions
- **Container Registry**: Docker Hub

## 🛠️ Prerequisites
Before you begin, ensure you have the following tools installed:
- Git
- Docker Desktop
- `kubectl` (Kubernetes command-line tool)
- `istioctl` (Istio command-line tool)

## 🚀 How to Run Locally (Docker Compose)

The easiest way to run the entire system on your local machine is with Docker Compose.

1.  **Navigate to the `src` directory**:
    ```bash
    cd src
    ```

2.  **Start all services**: This command will pull all necessary images and start every container. The first run may take several minutes.
    ```bash
    docker-compose -f docker-compose.yml -f docker-compose.local.yml up
    ```

3.  **Access the application**: Once the services are running, open your web browser and go to:
    `http://localhost:7005`

4.  **Stop all services**: To stop and remove all running containers, open a new terminal in the `src` directory and run:
    ```bash
    docker-compose down
    ```

## ☁️ How to Deploy to Kubernetes (AKS)

This guide assumes you have an Azure Kubernetes Service (AKS) cluster running and your `kubectl` is configured to connect to it.

1.  **Install Istio**: Before deploying the application, you must install the Istio control plane on your cluster. Navigate to the `istio` directory and run the install script.
    ```bash
    cd src/k8s/istio
    chmod +x install-istio.sh
    ./install-istio.sh
    ```

2.  **Deploy the Application**: The provided `start-all.sh` script will deploy all the infrastructure and application services to your cluster.
    ```bash
    cd ../scripts  # Navigate to the scripts directory
    chmod +x start-all.sh
    ./start-all.sh
    ```

3.  **Access the Application**: Find the public IP address of the Istio Ingress Gateway.
    ```bash
    kubectl get svc istio-ingressgateway -n istio-system
    ```
    Copy the `EXTERNAL-IP` and paste it into your web browser to see your live application.

## 🔄 CI/CD Pipeline
This repository is configured with a GitHub Actions workflow located at `.github/workflows/deploy-to-aks.yml`. This pipeline automates the entire process of building, pushing to Docker Hub, and deploying any changes to the AKS cluster.