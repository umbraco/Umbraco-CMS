import { rest } from 'msw';
import { umbTemplateData } from '../data/template.data';
import { umbracoPath } from '@umbraco-cms/utils';

// TODO: add schema
export const handlers = [
	rest.get(umbracoPath('/tree/template/root'), (req, res, ctx) => {
		const response = umbTemplateData.getTreeRoot();
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get(umbracoPath('/tree/template/children'), (req, res, ctx) => {
		const parentKey = req.url.searchParams.get('parentKey');
		if (!parentKey) return;
		const response = umbTemplateData.getTreeItemChildren(parentKey);
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get(umbracoPath('/tree/template/item'), (req, res, ctx) => {
		const keys = req.url.searchParams.getAll('key');
		if (!keys) return;

		const items = umbTemplateData.getTreeItem(keys);

		return res(ctx.status(200), ctx.json(items));
	}),
];
