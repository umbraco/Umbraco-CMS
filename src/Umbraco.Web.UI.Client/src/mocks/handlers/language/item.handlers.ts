const { rest } = window.MockServiceWorker;
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
];
