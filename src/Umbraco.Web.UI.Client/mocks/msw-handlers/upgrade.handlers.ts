const { http, HttpResponse } = window.MockServiceWorker;

import { umbracoPath } from '@umbraco-cms/backoffice/utils';
import type { UpgradeSettingsResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

export const handlers = [
	http.get(umbracoPath('/upgrade/settings'), () => {
		return HttpResponse.json<UpgradeSettingsResponseModel>({
			currentState: '2b20c6e7',
			newState: '2b20c6e8',
			oldVersion: '13.0.0',
			newVersion: '17.0.0',
			reportUrl: 'https://our.umbraco.com/download/releases/1700',
		});
	}),

	http.post(umbracoPath('/upgrade/authorize'), async () => {
		await new Promise((resolve) => setTimeout(resolve, (Math.random() + 1) * 1000)); // simulate a delay of 1-2 seconds

		return new HttpResponse(null, { status: 201 });
	}),
];
