import { body, defaultResponse, endpoint, request, response } from '@airtasker/spot';

import { ProblemDetails } from './models';

@endpoint({
	method: 'GET',
	path: '/upgrade/settings',
})
export class GetUpgradeSettings {
	@response({ status: 200 })
	success(@body body: UpgradeSettingsResponse) {}

	@defaultResponse
	default(@body body: ProblemDetails) {}
}

@endpoint({
	method: 'POST',
	path: '/upgrade/authorize',
})
export class PostUpgradeAuthorize {
	@request
	request() {}

	@response({ status: 201 })
	success() {}

	@response({ status: 400 })
	badRequest(@body body: ProblemDetails) {}
}

export interface UpgradeSettingsResponse {
	currentState: string;
	newState: string;
	newVersion: string;
	oldVersion: string;
	reportUrl: string;
}
