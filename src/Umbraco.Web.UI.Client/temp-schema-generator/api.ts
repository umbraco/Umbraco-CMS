import './installer';

import { api, body, defaultResponse, endpoint, request, response } from '@airtasker/spot';

import { ErrorResponse, InitResponse, UserLoginRequest, UserResponse, VersionResponse } from './models';

/* eslint-disable */
@api({ name: "umbraco-backoffice-api", version: "1.0.0" })
class Api { }

@endpoint({
  method: "GET",
  path: "/init",
})
class GetInit {
  @response({ status: 200 })
  success(@body body: InitResponse) { }

  @defaultResponse
  default(@body body: ErrorResponse) { }
}

@endpoint({
  method: "GET",
  path: "/version",
})
class GetVersion {
  @response({ status: 200 })
  success(@body body: VersionResponse) { }

  @defaultResponse
  default(@body body: ErrorResponse) { }
}

@endpoint({
  method: "POST",
  path: "/user/login",
})
class PostUserLogin {
  @request
  request(@body body: UserLoginRequest) { }

  @response({ status: 201 })
  success() { }

  @response({ status: 403 })
  failure(@body body: ErrorResponse) { }
}

@endpoint({
  method: "POST",
  path: "/user/logout",
})
class PostUserLogout {
  @response({ status: 201 })
  success() { }

  @defaultResponse
  default(@body body: ErrorResponse) { }
}

@endpoint({
  method: "GET",
  path: "/user",
})
class GetUser {
  @response({ status: 200 })
  success(@body body: UserResponse) { }

  @response({ status: 403 })
  failure(@body body: ErrorResponse) { }
}
