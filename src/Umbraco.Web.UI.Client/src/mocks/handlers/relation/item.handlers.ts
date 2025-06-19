const { rest } = window.MockServiceWorker;
import { umbRelationMockDb } from '../../data/relation/relation.db.js';
import type { GetRelationByRelationTypeIdResponse } from '@umbraco-cms/backoffice/external/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const itemHandlers = [
	rest.get(umbracoPath('/relation/type/:id'), (req, res, ctx) => {
		const skipParam = req.url.searchParams.get('skip');
		const skip = skipParam ? Number.parseInt(skipParam) : undefined;
		const takeParam = req.url.searchParams.get('take');
		const take = takeParam ? Number.parseInt(takeParam) : undefined;

		const response = umbRelationMockDb.get({ skip, take });
		response.items = response.items.filter((item) => item.relationType.id === req.params.id);
		response.total = response.items.length;

		return res(ctx.status(200), ctx.json<GetRelationByRelationTypeIdResponse>(response));
	}),
];
