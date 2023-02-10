import { rest } from 'msw';
import { umbCulturesData } from '../data/culture.data';
import { umbracoPath } from '@umbraco-cms/utils';

export const handlers = [
	rest.get(umbracoPath('/culture'), (req, res, ctx) => {
		const data = umbCulturesData.get();
		return res(ctx.status(200), ctx.json(data));
	}),
];
