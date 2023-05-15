import { rest } from 'msw';
import { umbDataTypeData } from '../../data/data-type.data';
import { slug } from './slug';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const itemHandlers = [
	rest.get(umbracoPath(`${slug}/item`), (req, res, ctx) => {
		const ids = req.url.searchParams.getAll('id');
		if (!ids) return;
		const items = umbDataTypeData.getItems(ids);
		return res(ctx.status(200), ctx.json(items));
	}),
];
