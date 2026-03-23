const { http, HttpResponse } = window.MockServiceWorker;

import { umbracoPath } from '@umbraco-cms/backoffice/utils';
import type { PagedTelemetryResponseModel, TelemetryResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { TelemetryLevelModel } from '@umbraco-cms/backoffice/external/backend-api';

let telemetryLevel = TelemetryLevelModel.BASIC;

export const handlers = [
	http.get(umbracoPath('/telemetry/level'), () => {
		return HttpResponse.json<TelemetryResponseModel>({
			telemetryLevel,
		});
	}),

	http.get(umbracoPath('/telemetry'), () => {
		return HttpResponse.json<PagedTelemetryResponseModel>({
			total: 3,
			items: [
				{ telemetryLevel: TelemetryLevelModel.MINIMAL },
				{ telemetryLevel: TelemetryLevelModel.BASIC },
				{ telemetryLevel: TelemetryLevelModel.DETAILED },
			],
		});
	}),

	http.post<object, TelemetryResponseModel>(umbracoPath('/telemetry/level'), async ({ request }) => {
		const body = await request.json();
		const newLevel = body?.telemetryLevel;
		if (newLevel) {
			telemetryLevel = newLevel;
			return new HttpResponse(null, { status: 200 });
		} else {
			return new HttpResponse(null, { status: 400 });
		}
	}),
];
