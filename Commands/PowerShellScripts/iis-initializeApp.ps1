Import-Module WebAdministration

#FUNCIONS DECLARATION - START
function CreateAppPool {
    Param([string] $name, [string] $runtimeVersion, [string] $identityName, [string] $identityPassword)

    if(Test-Path ("IIS:\AppPools\" + $name)) {
        Write-Host "The App Pool '$name' already exists. It creation has been skipped." -ForegroundColor Yellow
    } else {
        Write-Host "Creating App Pool '$name'" -ForegroundColor Cyan
        $appPool = New-WebAppPool -Name $name -Force
        $appPool.managedRuntimeVersion = $runtimeVersion
        $appPool.autoStart = "true"
        $appPool.processModel.userName = $identityName
        $appPool.processModel.password = $credential.GetNetworkCredential().Password
        $appPool.processModel.identityType = 3
        $appPool | Set-Item
    }
}

function CreateAppWebSite {
    Param([string] $name)

    if(Test-Path ("IIS:\Sites\$name")) {
        Write-Host "$name Web Site already exists. It creation has been skipped." -ForegroundColor Yellow
    } else {
        $webSitePhysicalPath = Read-Host "Enter the full path where your '$name' project is hosted. Example: C:\Users\myUserName\UciRod\$name"
        $ipAddress = Read-Host "Enter the ip address. For localhost: 127.0.0.1"
        [UInt32]$port = Read-Host "Enter the port. Example: 8080"

        Write-Host "Creating $name Web Site" -ForegroundColor Cyan
        New-WebSite -Name $name -ApplicationPool $name -PhysicalPath $webSitePhysicalPath -IPAddress $ipAddress -Port $port -Force
    }
}

function WebSiteContainsWebApplication {
    Param([string] $webSite, [string] $name)
    return (Get-WebApplication -Site $webSite -Name $name).Count -eq 1
}

function AddWebApplicationToWebSite {
    Param([string] $webSite, [string] $name)

    if(WebSiteContainsWebApplication -webSite $webSite -name $name) {
        Write-Host "$webSite Web Site already contains Web Application '$name'. It adding has been skipped." -ForegroundColor Yellow
    } else {
        Write-Host "Adding Web Application '$name' to $webSite Web Site" -ForegroundColor Cyan
        $webSitePhysicalPath = Get-Website $webSite | Select-Object -expa physicalPath
        New-WebApplication -Site $webSite -Name $name -ApplicationPool "$webSite.$name" -PhysicalPath "$webSitePhysicalPath\$name" -Force
    }
}
#FUNCIONS DECLARATION - END

$userName = "ENDAVA\URodriguez"
$secureIdentityPassword = Read-Host "Enter a Password for user identity '$userName'" -AsSecureString
$credential = New-Object System.Management.Automation.PSCredential($userName, $secureIdentityPassword)

$baseProjectName = "UciRod.Infrastructure"
$runtimeVersion = ""

#Initialize Application Pools
Write-Host "Initializing Application Pools" -ForegroundColor Cyan
CreateAppPool -name $baseProjectName -runtimeVersion $runtimeVersion -identityName $userName -identityPassword $credential.GetNetworkCredential().Password
CreateAppPool -name "$baseProjectName.Auditing" -runtimeVersion $runtimeVersion -identityName $userName -identityPassword $credential.GetNetworkCredential().Password
CreateAppPool -name "$baseProjectName.Authentication" -runtimeVersion $runtimeVersion -identityName $userName -identityPassword $credential.GetNetworkCredential().Password
CreateAppPool -name "$baseProjectName.Logging" -runtimeVersion $runtimeVersion -identityName $userName -identityPassword $credential.GetNetworkCredential().Password
CreateAppPool -name "$baseProjectName.Mailing" -runtimeVersion $runtimeVersion -identityName $userName -identityPassword $credential.GetNetworkCredential().Password
CreateAppPool -name "$baseProjectName.Rendering" -runtimeVersion $runtimeVersion -identityName $userName -identityPassword $credential.GetNetworkCredential().Password
Write-Host "Application Pools initialized" -ForegroundColor Green

#Initialize Web Sites
Write-Host "-------***-------" -ForegroundColor Cyan
Write-Host "Initializing Web Site" -ForegroundColor Cyan
CreateAppWebSite -name $baseProjectName
Write-Host "Web Site initialized" -ForegroundColor Green

#Initialize Web Applications
Write-Host "-------***-------" -ForegroundColor Cyan
Write-Host "Initializing Web Applications" -ForegroundColor Cyan
AddWebApplicationToWebSite -webSite $baseProjectName -name "Auditing"
AddWebApplicationToWebSite -webSite $baseProjectName -name "Authentication"
AddWebApplicationToWebSite -webSite $baseProjectName -name "Logging"
AddWebApplicationToWebSite -webSite $baseProjectName -name "Mailing"
AddWebApplicationToWebSite -webSite $baseProjectName -name "Rendering"
Write-Host "Web Applications initialized" -ForegroundColor Green