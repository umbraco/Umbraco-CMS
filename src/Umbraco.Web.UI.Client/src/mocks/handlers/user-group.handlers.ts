const { rest } = window.MockServiceWorker;
import { umbUserGroupData } from '../data/user-group.data.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

const slug = '/user-group';

export const handlers = [
	rest.get(umbracoPath(`${slug}/item`), (req, res, ctx) => {
		const ids = req.url.searchParams.getAll('id');
		if (!ids) return;
		const items = umbUserGroupData.getItems(ids);

		return res(ctx.status(200), ctx.json(items));
	}),

	rest.get(umbracoPath(`${slug}`), (req, res, ctx) => {
		const response = umbUserGroupData.getAll();

		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get(umbracoPath(`${slug}/:id`), (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return;
		const userGroup = umbUserGroupData.getById(id);

		return res(ctx.status(200), ctx.json(userGroup));
	}),

	rest.put(umbracoPath(`${slug}/:id`), async (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return;
		const data = await req.json();
		if (!data) return;

		umbUserGroupData.save(id, data);

		return res(ctx.status(200));
	}),
];
