import { rest } from 'msw';
import { umbTemplateData } from '../data/template.data';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';
import { CreateTemplateRequestModel, UpdateTemplateRequestModel } from '@umbraco-cms/backoffice/backend-api';

// TODO: add schema
export const handlers = [
	rest.get(umbracoPath('/tree/template/root'), (req, res, ctx) => {
		const response = umbTemplateData.getTreeRoot();
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get(umbracoPath('/tree/template/children'), (req, res, ctx) => {
		const parentId = req.url.searchParams.get('parentId');
		if (!parentId) return;

		const response = umbTemplateData.getTreeItemChildren(parentId);
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get(umbracoPath('/tree/template/item'), (req, res, ctx) => {
		const ids = req.url.searchParams.getAll('id');
		if (!ids) return;

		const items = umbTemplateData.getTreeItem(ids);
		return res(ctx.status(200), ctx.json(items));
	}),

	rest.get(umbracoPath('/template/scaffold'), (req, res, ctx) => {
		const masterTemplateAlias = req.url.searchParams.get('masterTemplateAlias');
		if (!masterTemplateAlias) return;

		const response = umbTemplateData.getScaffold(masterTemplateAlias);
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get(umbracoPath('/template/:id'), (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return;

		const response = umbTemplateData.getById(id);
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.put<UpdateTemplateRequestModel>(umbracoPath('/template/:id'), async (req, res, ctx) => {
		const id = req.params.id as string;
		const data = await req.json();
		if (!id) return;

		umbTemplateData.update(data);
		return res(ctx.status(200));
	}),

	rest.post<CreateTemplateRequestModel>(umbracoPath('/template'), async (req, res, ctx) => {
		const data = await req.json();
		if (!data) return;

		const created = umbTemplateData.create(data);
		return res(ctx.status(200), ctx.json(created));
	}),
];
