const { rest } = window.MockServiceWorker;
import { umbMediaTypeMockDb } from '../../data/media-type/media-type.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const itemHandlers = [
	rest.get(umbracoPath(`/item${UMB_SLUG}`), (req, res, ctx) => {
		const ids = req.url.searchParams.getAll('id');
		if (!ids) return;
		const items = umbMediaTypeMockDb.item.getItems(ids);
		return res(ctx.status(200), ctx.json(items));
	}),

	rest.get(umbracoPath(`/item${UMB_SLUG}/allowed`), (req, res, ctx) => {
		const fileExtension = req.url.searchParams.get('fileExtension');
		if (!fileExtension) return;

		const response = umbMediaTypeMockDb.getAllowedByFileExtension(fileExtension);

		return res(ctx.status(200), ctx.json(response));
	}),
];
