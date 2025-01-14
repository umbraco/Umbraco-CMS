const { rest } = window.MockServiceWorker;
import { umbRelationMockDb } from '../../data/relation/relation.db.js';
import { umbLanguageMockDb } from '../../data/language/language.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const itemHandlers = [
	rest.get(umbracoPath(`/item${UMB_SLUG}`), (req, res, ctx) => {
		const isoCodes = req.url.searchParams.getAll('id');
		if (!isoCodes) return;
		const items = umbLanguageMockDb.item.getItems(isoCodes);
		return res(ctx.status(200), ctx.json(items));
	}),

	rest.get(umbracoPath('/relation/type/:id'), (req, res, ctx) => {
		const skipParam = req.url.searchParams.get('skip');
		const skip = skipParam ? Number.parseInt(skipParam) : undefined;
		const takeParam = req.url.searchParams.get('take');
		const take = takeParam ? Number.parseInt(takeParam) : undefined;

		const response = umbRelationMockDb.get({ skip, take });
		response.items = response.items.filter((item) => item.relationType.id === req.params.id);
		response.total = response.items.length;

		return res(ctx.status(200), ctx.json(response));
	}),
];
