import { body, defaultResponse, endpoint, request, response } from '@airtasker/spot';

import { ProblemDetails } from './models';

@endpoint({
	method: 'GET',
	path: '/published-cache/status',
})
export class PublishedCacheStatus {
	@response({ status: 200 })
	success(@body body: string) {}

	@defaultResponse
	default(@body body: ProblemDetails) {}
}

@endpoint({
	method: 'POST',
	path: '/published-cache/reload',
})
export class PublishedCacheReload {
	@request
	request() {}

	@response({ status: 201 })
	success() {}

	@response({ status: 400 })
	badRequest(@body body: ProblemDetails) {}
}
