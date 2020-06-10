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

function CreateWebSite {
    Param([string] $name, [string] $appPool, [string] $physicalPath, [string] $ipAddress, [UInt32] $port)

    if(Test-Path ("IIS:\Sites\" + $name)) {
        Write-Host "The Web Site '$name' already exists. It creation has been skipped." -ForegroundColor Yellow
    } else {
        Write-Host "Creating Web Site '$name'" -ForegroundColor Cyan
        New-WebSite -Name $name -ApplicationPool $appPool -PhysicalPath $physicalPath -IPAddress $ipAddress -Port $port -Force
    }
}

function WebSiteContainsWebApplication {
    Param([string] $webSite, [string] $name)
    return (Get-WebApplication -Site $webSite -Name $name).Count -eq 1
}

function AddWebApplicationToWebSite {
    Param([string] $webSite, [string] $name, [string] $appPool, [string] $physicalPath)

    if(WebSiteContainsWebApplication -webSite $webSite -name $name) {
        Write-Host "The Web Site '$webSite' already contains Web Application '$name'. It adding has been skipped." -ForegroundColor Yellow
    } else {
        Write-Host "Adding Web Application '$name' to Web Site '$webSite'" -ForegroundColor Cyan
        New-WebApplication -Site $webSite -Name $name -ApplicationPool $appPool -PhysicalPath $physicalPath -Force
    }
}
#FUNCIONS DECLARATION - END

$userName = "ENDAVA\URodriguez"
$secureIdentityPassword = Read-Host "Enter a Password for user identity '$userName'" -AsSecureString
$credential = New-Object System.Management.Automation.PSCredential($userName, $secureIdentityPassword)

#Initialize Application Pools
Write-Host "Initializing Application Pools" -ForegroundColor Cyan
CreateAppPool -name "UciRod.Infrastructure" -runtimeVersion "" -identityName $userName -identityPassword $credential.GetNetworkCredential().Password
CreateAppPool -name "UciRod.Infrastructure.Auditing" -runtimeVersion "" -identityName $userName -identityPassword $credential.GetNetworkCredential().Password
CreateAppPool -name "UciRod.Infrastructure.Authentication" -runtimeVersion "" -identityName $userName -identityPassword $credential.GetNetworkCredential().Password
CreateAppPool -name "UciRod.Infrastructure.Logging" -runtimeVersion "" -identityName $userName -identityPassword $credential.GetNetworkCredential().Password
CreateAppPool -name "UciRod.Infrastructure.Mailing" -runtimeVersion "" -identityName $userName -identityPassword $credential.GetNetworkCredential().Password
CreateAppPool -name "UciRod.Infrastructure.Rendering" -runtimeVersion "" -identityName $userName -identityPassword $credential.GetNetworkCredential().Password
Write-Host "Application Pools initialized" -ForegroundColor Green

$projectPath = Read-Host "Enter the full path where your project is hosted. Example: C:\Users\userName\UciRod\ProjectName"
$ipAddress = Read-Host "Enter the ip address. For localhost: 127.0.0.1"
[UInt32]$port = Read-Host "Enter the port. Example: 8080"

#Initialize Web Sites
Write-Host "-------***-------" -ForegroundColor Cyan
Write-Host "Initializing Web Sites" -ForegroundColor Cyan
CreateWebSite -name "UciRod.Infrastructure" -appPool "UciRod.Infrastructure" -physicalPath $projectPath -ipAddress $ipAddress -port $port
Write-Host "Web Sites initialized" -ForegroundColor Green

#Initialize Web Applications
Write-Host "-------***-------" -ForegroundColor Cyan
Write-Host "Initializing Web Applications" -ForegroundColor Cyan
AddWebApplicationToWebSite -webSite "UciRod.Infrastructure" -name "Auditing" -appPool "UciRod.Infrastructure.Auditing" -physicalPath "$projectPath\Auditing"
AddWebApplicationToWebSite -webSite "UciRod.Infrastructure" -name "Authentication" -appPool "UciRod.Infrastructure.Authentication" -physicalPath "$projectPath\Authentication"
AddWebApplicationToWebSite -webSite "UciRod.Infrastructure" -name "Logging" -appPool "UciRod.Infrastructure.Logging" -physicalPath "$projectPath\Logging"
AddWebApplicationToWebSite -webSite "UciRod.Infrastructure" -name "Mailing" -appPool "UciRod.Infrastructure.Mailing" -physicalPath "$projectPath\Mailing"
AddWebApplicationToWebSite -webSite "UciRod.Infrastructure" -name "Rendering" -appPool "UciRod.Infrastructure.Rendering" -physicalPath "$projectPath\Rendering"
Write-Host "Web Applications initialized" -ForegroundColor Green