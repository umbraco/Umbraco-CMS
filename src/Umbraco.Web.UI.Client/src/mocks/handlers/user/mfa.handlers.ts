const { http, HttpResponse } = window.MockServiceWorker;
import { umbUserMockDb } from '../../data/user/user.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const handlers = [
	http.get(umbracoPath(`${UMB_SLUG}/:id/2fa`), () => {
		const mfaLoginProviders = umbUserMockDb.getMfaLoginProviders();
		return HttpResponse.json(mfaLoginProviders);
	}),
	http.delete(umbracoPath(`${UMB_SLUG}/:id/2fa/:providerName`), async ({ params }) => {
		const mfaLoginProviders = umbUserMockDb.getMfaLoginProviders();
		const provider = mfaLoginProviders.find((p) => p.providerName === params.providerName);
		if (!provider) {
			return new HttpResponse(null, { status: 404 });
		}

		provider.isEnabledOnUser = false;
		return new HttpResponse(null, { status: 200 });
	}),
];
