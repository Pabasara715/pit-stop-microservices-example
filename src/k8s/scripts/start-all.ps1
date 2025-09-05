# This script deploys the entire Pitstop application to Kubernetes.

param (
    [switch]$istio = $false,
    [string]$tag = "1.0"
)

$meshPostfix = ''
if ($istio) {
    $meshPostfix = '-istio'
    Write-Host "Starting Pitstop with Istio and Image Tag: $tag"
    # Disable global Istio side-car injection
    & "../istio/disable-default-istio-injection.ps1"
}
else {
    Write-Host "Starting Pitstop deployment without service mesh and Image Tag: $tag"
}

# Update the image tag in all Kubernetes deployment YAML files in the parent directory
Write-Host "Updating image tags in YAML files..."
$yamlFiles = Get-ChildItem -Path ".." -Filter "*.yaml"
$regex = "image: pabasaravihanga/pitstop-([a-zA-Z0-9\-]*):.*"
$replaceString = "image: pabasaravihanga/pitstop-`$1:$tag"

foreach ($file in $yamlFiles) {
    (Get-Content -Path $file.FullName) -replace $regex, $replaceString | Set-Content -Path $file.FullName
}

# Apply all Kubernetes manifests
Write-Host "Applying Kubernetes manifests..."
kubectl apply `
    -f ../pitstop-namespace$meshPostfix.yaml `
    -f ../rabbitmq.yaml `
    -f ../logserver.yaml `
    -f ../sqlserver$meshPostfix.yaml `
    -f ../mailserver.yaml `
    -f ../invoiceservice.yaml `
    -f ../timeservice.yaml `
    -f ../notificationservice.yaml `
    -f ../workshopmanagementeventhandler.yaml `
    -f ../auditlogservice.yaml `
    -f ../customermanagementapi$meshPostfix.yaml `
    -f ../customermanagementapi-svc.yaml `
    -f ../vehiclemanagementapi$meshPostfix.yaml `
    -f ../workshopmanagementapi$meshPostfix.yaml `
    -f ../webapp$meshPostfix.yaml

Write-Host "Deployment script finished."