const { rest } = window.MockServiceWorker;
import { umbUsersData } from '../../data/user.data.js';
import { slug } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const handlers = [
	rest.post(umbracoPath(`${slug}`), async (req, res, ctx) => {
		const data = await req.json();
		if (!data) return;

		const response = umbUsersData.createUser(data);

		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get(umbracoPath(`${slug}`), (req, res, ctx) => {
		const response = umbUsersData.getAll();
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get(umbracoPath(`${slug}/filter`), (req, res, ctx) => {
		//TODO: Implementer filter
		const response = umbUsersData.getAll();

		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get(umbracoPath(`${slug}/:id`), (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return;

		const item = umbUsersData.getById(id);

		return res(ctx.status(200), ctx.json(item));
	}),

	rest.put(umbracoPath(`${slug}/:id`), async (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return;
		const data = await req.json();
		if (!data) return;

		umbUsersData.save(id, data);

		return res(ctx.status(200));
	}),

	rest.delete<string>(umbracoPath(`${slug}/:id`), async (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return;

		umbUsersData.delete([id]);

		return res(ctx.status(200));
	}),
];
