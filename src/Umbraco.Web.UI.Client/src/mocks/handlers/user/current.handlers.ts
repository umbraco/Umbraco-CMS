const { rest } = window.MockServiceWorker;
import { umbUserMockDb } from '../../data/user/user.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const handlers = [
	rest.get(umbracoPath(`${UMB_SLUG}/current`), (_req, res, ctx) => {
		const loggedInUser = umbUserMockDb.getCurrentUser();
		return res(ctx.status(200), ctx.json(loggedInUser));
	}),
	rest.get(umbracoPath(`${UMB_SLUG}/current/2fa`), (_req, res, ctx) => {
		const mfaLoginProviders = umbUserMockDb.getMfaLoginProviders();
		return res(ctx.status(200), ctx.json(mfaLoginProviders));
	}),
	rest.get(umbracoPath(`${UMB_SLUG}/current/2fa/:providerName`), (req, res, ctx) => {
		if (!req.params.providerName) {
			return res(ctx.status(400));
		}

		const mfaProviders = umbUserMockDb.getMfaLoginProviders();
		const mfaProvider = mfaProviders.find((p) => p.providerName === req.params.providerName.toString());

		if (!mfaProvider) {
			return res(ctx.status(404));
		}

		return res(
			ctx.status(200),
			ctx.json({
				$type: 'TwoFactorAuthInfo',
				qrCodeSetupImageUrl: 'https://placekitten.com/200/200',
				secret: '8b713fc7-8f17-4f5d-b2ac-b53879c75953',
			}),
		);
	}),
	rest.post<{ code: string; secret: string }>(
		umbracoPath(`${UMB_SLUG}/current/2fa/:providerName`),
		async (req, res, ctx) => {
			const body = await req.json();
			if (!req.params.providerName || !body.code || !body.secret) {
				return res(ctx.status(400));
			}

			const result = umbUserMockDb.enableMfaProvider(req.params.providerName.toString());
			return res(ctx.status(result ? 200 : 404));
		},
	),
	rest.delete<{ code: string }>(umbracoPath(`${UMB_SLUG}/current/2fa/:providerName`), (req, res, ctx) => {
		if (!req.params.providerName) {
			return res(ctx.status(400));
		}

		const result = umbUserMockDb.disableMfaProvider(req.params.providerName.toString());
		return res(ctx.status(result ? 200 : 404));
	}),
];
