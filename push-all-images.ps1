# PowerShell script to tag and push all Pitstop images to Docker Hub
# Set your Docker Hub username
$DOCKERHUB_USER = "pabasaravihanga"

# List of services
$services = @(
    "auditlogservice",
    "customermanagementapi",
    "invoiceservice",
    "notificationservice",
    "timeservice",
    "vehiclemanagementapi",
    "webapp",
    "workshopmanagementapi",
    "workshopmanagementeventhandler"
)

foreach ($service in $services) {
    $local = "pitstop/$service:1.0"
    $remote = "$DOCKERHUB_USER/pitstop-$service:1.0"
    # Check if the local image exists
    $imageExists = docker images -q $local
    if ([string]::IsNullOrWhiteSpace($imageExists)) {
        Write-Host "Skipping $($local): image not found."
        continue
    }
    Write-Host "Tagging $local as $remote"
    docker tag $local $remote
    Write-Host "Pushing $remote"
    docker push $remote
}

Write-Host "\n---"
Write-Host "Update your Kubernetes manifests to use images like:"
Write-Host "  image: pabasaravihanga/pitstop-<servicename>:1.0"
Write-Host "Then redeploy with:"
Write-Host "  ./start-all.ps1 -istio"
Write-Host "---"
