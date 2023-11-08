const { rest } = window.MockServiceWorker;
import { umbMediaTypeData } from '../../data/media-type.data.js';
import { slug } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const itemHandlers = [
	rest.get(umbracoPath(`${slug}/item`), (req, res, ctx) => {
		const ids = req.url.searchParams.getAll('id');
		if (!ids) return;
		const items = umbMediaTypeData.getItems(ids);
		return res(ctx.status(200), ctx.json(items));
	}),

	rest.get(umbracoPath(`/tree${slug}/item`), (req, res, ctx) => {
		const ids = req.url.searchParams.getAll('id');
		if (!ids) return;
		const items = umbMediaTypeData.getTreeItems(ids);
		return res(ctx.status(200), ctx.json(items));
	}),
];
