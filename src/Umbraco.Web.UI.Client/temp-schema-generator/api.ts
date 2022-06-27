import './installer';
import './user';

import { api, body, defaultResponse, endpoint, response } from '@airtasker/spot';

import { InitResponse, ProblemDetails, VersionResponse } from './models';

/* eslint-disable */
@api({ name: 'umbraco-backoffice-api', version: '1.0.0' })
class Api {}

@endpoint({
  method: 'GET',
  path: '/init',
})
class GetInit {
  @response({ status: 200 })
  success(@body body: InitResponse) {}

  @defaultResponse
  default(@body body: ProblemDetails) {}
}

@endpoint({
  method: 'GET',
  path: '/version',
})
class GetVersion {
  @response({ status: 200 })
  success(@body body: VersionResponse) {}

  @defaultResponse
  default(@body body: ProblemDetails) {}
}
