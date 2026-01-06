const { http, HttpResponse } = window.MockServiceWorker;
import { umbUserMockDb } from '../../data/user/user.db.js';
import { UMB_SLUG } from './slug.js';
import type { GetUserCurrentLoginProvidersResponse } from '@umbraco-cms/backoffice/external/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const handlers = [
	http.get(umbracoPath(`${UMB_SLUG}/current`), () => {
		const loggedInUser = umbUserMockDb.getCurrentUser();
		return HttpResponse.json(loggedInUser);
	}),
	http.get<GetUserCurrentLoginProvidersResponse>(umbracoPath(`${UMB_SLUG}/current/login-providers`), () => {
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
	http.get(umbracoPath(`${UMB_SLUG}/current/2fa/:providerName`), ({ params }) => {
		if (!params.providerName) {
			return new HttpResponse(null, { status: 400 });
		}

		const mfaProviders = umbUserMockDb.getMfaLoginProviders();
		const mfaProvider = mfaProviders.find((p) => p.providerName === params.providerName.toString());

		if (!mfaProvider) {
			return new HttpResponse(null, { status: 404 });
		}

		return HttpResponse.json({
			$type: 'TwoFactorAuthInfo',
			qrCodeSetupImageUrl: 'https://placehold.co/200x200?text=QR+Code+here',
			secret: '8b713fc7-8f17-4f5d-b2ac-b53879c75953',
		});
	}),
	http.post<{ code: string; secret: string }>(
		umbracoPath(`${UMB_SLUG}/current/2fa/:providerName`),
		async ({ request, params }) => {
			const body = await request.json();
			if (!params.providerName || !body.code || !body.secret) {
				return new HttpResponse(null, { status: 400 });
			}

			if (body.code === 'fail') {
				return new HttpResponse(null, { status: 400 });
			}

			const result = umbUserMockDb.enableMfaProvider(params.providerName.toString());
			return new HttpResponse(null, { status: result ? 200 : 404 });
		},
	),
	http.delete<{ code: string }>(umbracoPath(`${UMB_SLUG}/current/2fa/:providerName`), ({ request, params }) => {
		const url = new URL(request.url);
		const code = url.searchParams.get('code');
		if (!params.providerName || !code) {
			return new HttpResponse(null, { status: 400 });
		}

		if (code === 'fail') {
			return new HttpResponse(null, { status: 400 });
		}

		const result = umbUserMockDb.disableMfaProvider(params.providerName.toString());
		return new HttpResponse(null, { status: result ? 200 : 404 });
	}),
];
