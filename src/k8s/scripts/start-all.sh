#!/bin/bash

# Default values
IMAGE_TAG="1.0"
ISTIO=false
MESH_POSTFIX=""

# Parse command-line arguments
while [[ "$#" -gt 0 ]]; do
    case $1 in
        --istio) ISTIO=true ;;
        --tag) IMAGE_TAG="$2"; shift ;;
        *) echo "Unknown parameter passed: $1"; exit 1 ;;
    esac
    shift
done

if [ "$ISTIO" = true ]; then
    MESH_POSTFIX="-istio"
    echo "Starting Pitstop deployment with Istio and Image Tag: $IMAGE_TAG"
    # Disable global Istio side-car injection
    ../istio/disable-default-istio-injection.sh
else
    echo "Starting Pitstop deployment without service mesh and Image Tag: $IMAGE_TAG"
fi

# Update the image tag in all Kubernetes deployment YAML files in the parent directory
echo "Updating image tags in YAML files..."
for yaml in ../*.yaml; do
    if [ -f "$yaml" ]; then
        # This regex finds 'image: username/pitstop-servicename:anything' and replaces the tag
        sed -i "s|image: pabasaravihanga/pitstop-\([a-zA-Z0-9\-]*\):.*|image: pabasaravihanga/pitstop-\1:$IMAGE_TAG|g" "$yaml"
    fi
done

# Apply all Kubernetes manifests
echo "Applying Kubernetes manifests..."
kubectl apply \
    -f ../pitstop-namespace$MESH_POSTFIX.yaml \
    -f ../rabbitmq.yaml \
    -f ../logserver.yaml \
    -f ../sqlserver$MESH_POSTFIX.yaml \
    -f ../mailserver.yaml \
    -f ../invoiceservice.yaml \
    -f ../timeservice.yaml \
    -f ../notificationservice.yaml \
    -f ../workshopmanagementeventhandler.yaml \
    -f ../auditlogservice.yaml \
    -f ../customermanagementapi$MESH_POSTFIX.yaml \
    -f ../customermanagementapi-svc.yaml \
    -f ../vehiclemanagementapi$MESH_POSTFIX.yaml \
    -f ../workshopmanagementapi$MESH_POSTFIX.yaml \
    -f ../webapp$MESH_POSTFIX.yaml

echo "Deployment script finished."