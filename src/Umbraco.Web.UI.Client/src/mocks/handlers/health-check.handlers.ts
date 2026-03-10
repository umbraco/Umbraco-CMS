const { http, HttpResponse } = window.MockServiceWorker;

import {
	getGroupByName,
	getGroupWithResultsByName,
	healthGroups,
	healthGroupsWithoutResult,
} from '../data/health-check.data.js';

import type {
	HealthCheckActionRequestModel,
	HealthCheckGroupResponseModel,
	HealthCheckGroupWithResultResponseModel,
	HealthCheckResultResponseModel,
	PagedHealthCheckGroupResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { StatusResultTypeModel } from '@umbraco-cms/backoffice/external/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

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

	http.post<object, HealthCheckActionRequestModel>(umbracoPath('/health-check/execute-action'), async ({ request }) => {
		const body = await request.json();
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
