const { rest } = window.MockServiceWorker;
import { umbDocumentTypeData } from '../../data/document-type/document-type.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const handlers = [
	rest.get(umbracoPath(`${UMB_SLUG}/item`), (req, res, ctx) => {
		const ids = req.url.searchParams.getAll('id');
		if (!ids) return;
		const items = umbDocumentTypeData.getItems(ids);
		return res(ctx.status(200), ctx.json(items));
	}),
];
