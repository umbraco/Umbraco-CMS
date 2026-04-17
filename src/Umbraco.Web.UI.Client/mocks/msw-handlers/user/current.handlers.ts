const { http, HttpResponse } = window.MockServiceWorker;
import { umbUserMockDb } from '../../db/user.db.js';
import { UMB_SLUG } from './slug.js';
import type { GetUserCurrentLoginProvidersResponse } from '@umbraco-cms/backoffice/external/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const handlers = [
	http.get(umbracoPath(`${UMB_SLUG}/current`), () => {
		const loggedInUser = umbUserMockDb.getCurrentUser();
		return HttpResponse.json(loggedInUser);
	}),
	http.get(umbracoPath(`${UMB_SLUG}/current/login-providers`), () => {
		return HttpResponse.json<GetUserCurrentLoginProvidersResponse>([
			{
				hasManualLinkingEnabled: true,
				isLinkedOnUser: true,
				providerKey: 'google',
				providerSchemeName: 'Umbraco.Google',
			},
		]);
	}),
	http.get(umbracoPath(`${UMB_SLUG}/current/2fa`), () => {
		const mfaLoginProviders = umbUserMockDb.getMfaLoginProviders();
		return HttpResponse.json(mfaLoginProviders);
	}),
	http.get<{ providerName: string }>(umbracoPath(`${UMB_SLUG}/current/2fa/:providerName`), ({ params }) => {
		const providerName = params.providerName;
		if (!providerName) {
			return new HttpResponse(null, { status: 400 });
		}

		const mfaProviders = umbUserMockDb.getMfaLoginProviders();
		const mfaProvider = mfaProviders.find((p) => p.providerName === providerName);

		if (!mfaProvider) {
			return new HttpResponse(null, { status: 404 });
		}

		return HttpResponse.json({
			$type: 'TwoFactorAuthInfo',
			qrCodeSetupImageUrl: 'https://placehold.co/200x200?text=QR+Code+here',
			secret: '8b713fc7-8f17-4f5d-b2ac-b53879c75953',
		});
	}),
	http.post<{ providerName: string }, { code: string; secret: string }>(
		umbracoPath(`${UMB_SLUG}/current/2fa/:providerName`),
		async ({ request, params }) => {
			const providerName = params.providerName;
			const body = await request.json();
			if (!providerName || !body.code || !body.secret) {
				return new HttpResponse(null, { status: 400 });
			}

			if (body.code === 'fail') {
				return new HttpResponse(null, { status: 400 });
			}

			const result = umbUserMockDb.enableMfaProvider(providerName);
			return new HttpResponse(null, { status: result ? 200 : 404 });
		},
	),
	http.delete<{ providerName: string }>(umbracoPath(`${UMB_SLUG}/current/2fa/:providerName`), ({ request, params }) => {
		const providerName = params.providerName;
		const url = new URL(request.url);
		const code = url.searchParams.get('code');
		if (!providerName || !code) {
			return new HttpResponse(null, { status: 400 });
		}

		if (code === 'fail') {
			return new HttpResponse(null, { status: 400 });
		}

		const result = umbUserMockDb.disableMfaProvider(providerName);
		return new HttpResponse(null, { status: result ? 200 : 404 });
	}),
];
