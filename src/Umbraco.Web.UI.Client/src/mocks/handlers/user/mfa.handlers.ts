const { rest } = window.MockServiceWorker;
import { umbUserMockDb } from '../../data/user/user.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const handlers = [
	rest.get(umbracoPath(`${UMB_SLUG}/:id/2fa`), (_req, res, ctx) => {
		const mfaLoginProviders = umbUserMockDb.getMfaLoginProviders();
		return res(ctx.status(200), ctx.json(mfaLoginProviders));
	}),
	rest.delete(umbracoPath(`${UMB_SLUG}/:id/2fa/:providerName`), async (req, res, ctx) => {
		const mfaLoginProviders = umbUserMockDb.getMfaLoginProviders();
		const provider = mfaLoginProviders.find((p) => p.providerName === req.params.providerName);
		if (!provider) {
			return res(ctx.status(404));
		}

		provider.isEnabledOnUser = false;
		return res(ctx.status(200));
	}),
];
