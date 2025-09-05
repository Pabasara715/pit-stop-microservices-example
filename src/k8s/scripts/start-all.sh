#!/bin/bash

kubectl apply \
echo "Starting Pitstop with Istio service mesh."

# disable global istio side-car injection (only for annotated pods)
../istio/disable-default-istio-injection.sh


    -f ../pitstop-namespace-istio.yaml \
    -f ../rabbitmq.yaml \
    -f ../logserver.yaml \
    -f ../sqlserver-istio.yaml \
    -f ../mailserver.yaml \
    -f ../invoiceservice.yaml \
    -f ../timeservice.yaml \
    -f ../notificationservice.yaml \
    -f ../workshopmanagementeventhandler.yaml \
    -f ../auditlogservice.yaml \
    -f ../customermanagementapi-istio.yaml \
    -f ../customermanagementapi-svc.yaml \
    -f ../vehiclemanagementapi-istio.yaml \
    -f ../workshopmanagementapi-istio.yaml \
    -f ../webapp-istio.yaml
    echo "Starting Pitstop with Istio service mesh."
