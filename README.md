# infrastructure
Since almost every application needs some infrastructure services which don not related to the business login, having them and just reuse them is essential. this repository tries to cover these services.

the overall view is every request goes though the gateway to then is being redirected to the specific service base on the requested path
Example :
request `api/v1/auth/login` to gateway after required validation goes to `api/v1/login` endpoint of the auth service.