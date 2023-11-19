const { rest } = window.MockServiceWorker;
import { umbDataTypeData } from '../../data/data-type.data.js';
import { UMB_slug } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const itemHandlers = [
	rest.get(umbracoPath(`${UMB_slug}/item`), (req, res, ctx) => {
		const ids = req.url.searchParams.getAll('id');
		if (!ids) return;
		const items = umbDataTypeData.getItems(ids);
		return res(ctx.status(200), ctx.json(items));
	}),
];
