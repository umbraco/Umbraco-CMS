const { http, HttpResponse } = window.MockServiceWorker;

import { dataSet } from '../data/sets/index.js';
import type {
	HealthCheckActionRequestModel,
	HealthCheckGroupResponseModel,
	HealthCheckGroupWithResultResponseModel,
	HealthCheckResultResponseModel,
	PagedHealthCheckGroupResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { StatusResultTypeModel } from '@umbraco-cms/backoffice/external/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

const healthGroups = dataSet.healthGroups ?? [];
const healthGroupsWithoutResult = dataSet.healthGroupsWithoutResult ?? [];

/**
 *
 * @param name
 */
function getGroupByName(name: string) {
	return healthGroupsWithoutResult.find((group) => group.name?.toLowerCase() == name.toLowerCase());
}

/**
 *
 * @param name
 */
export function getGroupWithResultsByName(name: string) {
	return healthGroups.find((group) => group.name.toLowerCase() === name.toLowerCase());
}

export const handlers = [
	http.get(umbracoPath('/health-check-group'), () => {
		return HttpResponse.json<PagedHealthCheckGroupResponseModel>({ total: 9999, items: healthGroupsWithoutResult });
	}),

	http.get(umbracoPath('/health-check-group/:name'), ({ params }) => {
		const name = params.name as string;

		if (!name) return;
		const group = getGroupByName(name);

		if (group) {
			return HttpResponse.json<HealthCheckGroupResponseModel>(group);
		} else {
			return new HttpResponse(null, { status: 404 });
		}
	}),

	http.post(umbracoPath('/health-check-group/:name/check'), ({ params }) => {
		const name = params.name as string;
		if (!name) return;

		const group = getGroupWithResultsByName(name);

		if (group) {
			return HttpResponse.json<HealthCheckGroupWithResultResponseModel>(group);
		} else {
			return new HttpResponse(null, { status: 404 });
		}
	}),

	http.post<HealthCheckActionRequestModel>(umbracoPath('/health-check/execute-action'), async ({ request }) => {
		const body = await request.json<HealthCheckActionRequestModel>();
		const healthCheckId = body.healthCheck.id;
		// Find the health check based on the healthCheckId from the healthGroups[].checks
		const healthCheck = healthGroups.flatMap((group) => group.checks).find((check) => check?.id === healthCheckId);

		if (!healthCheck) {
			return new HttpResponse(null, { status: 404 });
		}

		const result = healthCheck.results?.at(0);

		if (!result) {
			return new HttpResponse(null, { status: 404 });
		}

		result.resultType = StatusResultTypeModel.SUCCESS;

		// Note: ctx.delay() is not directly supported in v2, needs to be implemented differently if delay is needed
		await new Promise((resolve) => setTimeout(resolve, 1000));
		return HttpResponse.json<HealthCheckResultResponseModel>(result);
	}),
];
