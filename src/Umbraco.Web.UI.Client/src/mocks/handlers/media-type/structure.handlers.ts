const { rest } = window.MockServiceWorker;
import { umbMediaTypeMockDb } from '../../data/media-type/media-type.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const structureHandlers = [
	rest.get(umbracoPath(`${UMB_SLUG}/:id/allowed-children`), (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return res(ctx.status(400));
		const response = umbMediaTypeMockDb.getAllowedChildren(id);
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get(umbracoPath(`${UMB_SLUG}/allowed-at-root`), (req, res, ctx) => {
		const response = umbMediaTypeMockDb.getAllowedAtRoot();
		return res(ctx.status(200), ctx.json(response));
	}),
];
