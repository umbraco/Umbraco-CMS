import { body, defaultResponse, endpoint, response } from '@airtasker/spot';

import { ProblemDetails, StatusResponse, VersionResponse } from './models';

@endpoint({
  method: 'GET',
  path: '/server/status',
})
export class GetStatus {
  @response({ status: 200 })
  success(@body body: StatusResponse) {}

  @defaultResponse
  default(@body body: ProblemDetails) {}
}

@endpoint({
  method: 'GET',
  path: '/server/version',
})
export class GetVersion {
  @response({ status: 200 })
  success(@body body: VersionResponse) {}

  @defaultResponse
  default(@body body: ProblemDetails) {}
}
