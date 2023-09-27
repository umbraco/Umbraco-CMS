const { rest } = window.MockServiceWorker;
import { umbUserGroupData } from '../../data/user-group.data.js';
import { slug } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const detailHandlers = [
	rest.post(umbracoPath(`${slug}`), async (req, res, ctx) => {
		const data = await req.json();
		if (!data) return;

		umbUserGroupData.insert(data);

		return res(ctx.status(200));
	}),

	rest.get(umbracoPath(`${slug}/:id`), (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return;

		const dataType = umbUserGroupData.getById(id);

		return res(ctx.status(200), ctx.json(dataType));
	}),

	rest.put(umbracoPath(`${slug}/:id`), async (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return;
		const data = await req.json();
		if (!data) return;

		umbUserGroupData.save(id, data);

		return res(ctx.status(200));
	}),

	rest.delete<string>(umbracoPath(`${slug}/:id`), async (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return;

		umbUserGroupData.delete([id]);

		return res(ctx.status(200));
	}),
];
