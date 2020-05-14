# Infrastructure Project
Infrastructure solutions for UciRod

# Infrastructure URL - ENV
http://localhost:8082 -> Jenkins

http://www.ucirod.infrastructure-test.com:40000 -> Test (IIS Local - overwritten in C:\Windows\System32\drivers\etc\hosts)

## Logging URL - ENV
https://localhost:44330 -> Dev (IIS Express - https)
https://localhost:44330/hangfire
http://www.ucirod.infrastructure-test.com:40000/logging
http://www.ucirod.infrastructure-test.com:40000/logging/hangfire

## Auditing URL - ENV
https://localhost:44387 -> Dev (IIS Express - https)
http://www.ucirod.infrastructure-test.com:40000/auditing

## Mailing URL - ENV
https://localhost:44386/ -> Dev (IIS Express - https)
http://www.ucirod.infrastructure-test.com:40000/mailing

## Authentication URL - ENV
https://localhost:44315 -> Dev (IIS Express - https)
http://www.ucirod.infrastructure-test.com:40000/authentication

## TODO list
* API Gateway
* create client application (React)
  * user register -> receive email
  * user login
  * user see list available infrastructure services
  * user select infrastructure services
  * user pay -> credit card transactions?
  * logs module
* Authentication: implement refresh token
* deploy app to cloud - PROD env
* enqueue failed data
* dequeue failed data and resend

## DONE list
* Authentication: implemenent
* Authentication: deploy
* Mailing: implement
* Authentication:differentiate between invalid token or expired token exception (catching corrrect exception)
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