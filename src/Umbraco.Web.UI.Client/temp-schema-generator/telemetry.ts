import { body, defaultResponse, endpoint, request, response } from '@airtasker/spot';

import { ProblemDetails } from './models';

@endpoint({
	method: 'GET',
	path: '/telemetry/ConsentLevel',
})
export class GetConsentLevel {
	@response({ status: 200 })
	success(@body body: ConsentLevelSettings) {}

	@defaultResponse
	default(@body body: ProblemDetails) {}
}

@endpoint({
	method: 'GET',
	path: '/telemetry/ConsentLevels',
})
export class ConsentLevels {
	@response({ status: 200 })
	success(@body body: string[]) {}

	@defaultResponse
	default(@body body: ProblemDetails) {}
}

@endpoint({
	method: 'POST',
	path: '/telemetry/ConsentLevel',
})
export class PostConsentLevel {
	@request
	request(@body body: ConsentLevelSettings) {}

	@response({ status: 201 })
	success() {}

	@response({ status: 400 })
	badRequest(@body body: ProblemDetails) {}
}

export interface ConsentLevelSettings {
	telemetryLevel: string;
}
