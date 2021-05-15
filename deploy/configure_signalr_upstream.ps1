param (
    [Parameter(Mandatory=$True)]
    [String]$FunctionAppName,
    
    [Parameter(Mandatory=$True)]
    [String]$SignalRName,

    [Parameter(Mandatory=$True)]
    [String]$ResourceGroup
)

Write-Host "Retrieving Function App information"

$functionAppDetails = (az functionapp show --name $FunctionAppName --resource-group $ResourceGroup) | ConvertFrom-Json

if( $null -eq $functionAppDetails )
{
    Write-Error "No FunctionApp found with name $FunctionAppName in resource-group $ResourceGroup"
}

$secretKeyAndConnectionstring = (az functionapp keys list --resource-group $ResourceGroup --name $FunctionAppName) | ConvertFrom-Json

if( $null -eq $secretKeyAndConnectionstring )
{
    Write-Error "Could not retrieve Function App Keys for Function App $FunctionAppName"
}

if( $null -eq $secretKeyAndConnectionstring.systemKeys.signalr_extension )
{
    Write-Error "No signalr_extension key found on Function App $FunctionAppName"
}

$signalRKey = $secretKeyAndConnectionstring.systemKeys.signalr_extension


$telemetryNotifierUrlTemplate = "https://$($functionAppDetails.defaultHostname)/runtime/webhooks/signalr?code=$signalRKey"

Write-Host "Configure SignalR Upstream configuration"
az signalr upstream update --name $SignalRName --resource-group $ResourceGroup --template url-template="$telemetryNotifierUrlTemplate" 
Write-Host "Upstream for $SignalRName configured!"
