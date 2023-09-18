const { rest } = window.MockServiceWorker;

import { umbUsersData } from '../data/user.data.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

const slug = '/user';

export const handlers = [
	rest.get(umbracoPath(`${slug}/filter`), (req, res, ctx) => {
		//TODO: Implementer filter
		const response = umbUsersData.getAll();

		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get(umbracoPath(`${slug}/current`), (_req, res, ctx) => {
		const loggedInUser = umbUsersData.getCurrentUser();
		return res(ctx.status(200), ctx.json(loggedInUser));
	}),

	rest.get(umbracoPath(`${slug}/sections`), (_req, res, ctx) => {
		return res(
			ctx.status(200),
			ctx.json({
				sections: ['Umb.Section.Content', 'Umb.Section.Media', 'Umb.Section.Settings', 'My.Section.Custom'],
			}),
		);
	}),

	rest.get(umbracoPath(`${slug}`), (req, res, ctx) => {
		const response = umbUsersData.getAll();

		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get(umbracoPath(`${slug}/:id`), (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return;
		const user = umbUsersData.getById(id);

		if (!user) return res(ctx.status(404));

		return res(ctx.status(200), ctx.json(user));
	}),

	rest.put(umbracoPath(`${slug}/:id`), async (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return;
		const data = await req.json();
		if (!data) return;

		const saved = umbUsersData.save(id, data);

		return res(ctx.status(200), ctx.json(saved));
	}),
];
