import { items } from '../data/tracked-reference.data.js';
const { rest } = window.MockServiceWorker;
import { umbracoPath } from '@umbraco-cms/backoffice/utils';
import { PagedRelationItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

export const handlers = [
	rest.get(umbracoPath('/tracked-reference/:id'), (_req, res, ctx) => {
		const id = _req.params.id as string;
		if (!id) return;

		const PagedTrackedReference = {
			total: items.length,
			items: items,
		};

		return res(ctx.status(200), ctx.json<PagedRelationItemResponseModel>(PagedTrackedReference));
	}),
];
