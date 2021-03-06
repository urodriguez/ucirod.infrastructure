# Infrastructure Project
Infrastructure solutions for UciRod

# UciRod - CI/CD - Jenkins - ip: 127.0.0.1 - www.ucirod.jenkins.com mapped in C:\Windows\System32\drivers\etc\hosts
http://www.ucirod.jenkins.com:8082 | user: ucirod, password: ucirod

## Base URL - ENV - ip: 192.168.0.239 (internal private ip) - IIS - www.ucirod.infrastructure-test.com mapped in C:\Windows\System32\drivers\etc\hosts
www.ucirod.infrastructure-test.com:8081

## Logging - ENV
https://localhost:44330 -> Dev (IIS Express - https)
https://localhost:44330/hangfire
http://152.171.94.90:8081/logging
http://152.171.94.90:8081/logging/hangfire -> use cookie = { Name = "infrastructure_hf_dashboard_cookie", Value = "1nfr45tructur3_h4ngf1r3_d45hb0rd" }

## Auditing - ENV
https://localhost:44387 -> Dev (IIS Express - https)
http://152.171.94.90:8081/auditing

## Mailing - ENV
https://localhost:44386 -> Dev (IIS Express - https)
http://152.171.94.90:8081/mailing

## Authentication - ENV
https://localhost:44315 -> Dev (IIS Express - https)
http://152.171.94.90:8081/authentication

## Reporting - ENV
https://localhost:44341 -> Dev (IIS Express - https)
http://152.171.94.90:8081/reporting

## TODO list
* Auditing: e2e test for API
* Storing: implement - allow ApplicationSettings, folders and files
* Create PortalApi
* Create Portal Web Client Application (React)
  * user register -> receive email
  * user login
  * user see list available infrastructure services
  * user select infrastructure services
  * user pay -> credit card transactions?
  * logs module
* Authentication: implement refresh token
* handle massive logs rows on Log table
* API Gateway
* deploy app to cloud - PROD env
* enqueue failed data
* dequeue failed data and resend
* Docker: migrate Projects using docker compose

## DONE list
* Authentication: implemenent
* Authentication: deploy
* Mailing: implement
* Authentication: differentiate between invalid token or expired token exception (catching corrrect exception)
* Authentication: reduce exp token to 1h
* use credencials validation in all products
* Jenkins: stop app pool before build
* create BaseApiController and do credentials validation there
* update scripts to be idempotent
* Jenkins - full deploy + improve powershell scripts
* Auditing: 'auditV2'move logic to process old entity (storing the current state)
* Auditing: handling exceptions-> DONE
* avoid show internal message errors, only log them. Show reference Id
* Logging: remove correlations endpoint + make it optional on dto and string
* Logging: log locally Correlation & LoggingDb
* Logging: implement process to delete old logs (database and file system)
* versioning all projects
* Reporting: implement
* Mailing: allow attachments
* Auditing: implement complex auditing - objects with nested objects
* Auditing: implement complex auditing - objects with nested arrays of objects
* Auditing: test on TEST env + fix bugs
* Auditing: feature to add/remove elements on object/array
* Jenkins: add sql script to build process
* expose infrastructure services to external (no localhost) URL via public ip for TEST env
* organize readme by ENV
* refactor AppSettingsService
* make blocking methods (database, external webservices, I/O) async => spread to controllers, app services, repositories, etc
* expose hangfire dashboard to allow external request - https://docs.hangfire.io/en/latest/configuration/using-dashboard.html#configuring-authorization
* Rename Reporting to Rendering + allow outputType: pdf and html
* create script to configure sites on IIS
* use Docker - SqlServer
* Migrate Projects to netcore v3.1