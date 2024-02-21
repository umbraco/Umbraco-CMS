const { rest } = window.MockServiceWorker;
import { umbMediaMockDb } from '../../data/media/media.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const collectionHandlers = [
	rest.get(umbracoPath(`/collection${UMB_SLUG}`), (req, res, ctx) => {
		const id = req.url.searchParams.get('id') ?? '';

		const skip = Number(req.url.searchParams.get('skip'));
		const take = Number(req.url.searchParams.get('take'));

		const response = umbMediaMockDb.collection.getCollectionMedia({ id, skip, take });

		return res(ctx.status(200), ctx.json(response));
	}),
];
