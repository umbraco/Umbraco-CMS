const { rest } = window.MockServiceWorker;

import { umbUsersData } from '../data/users.data.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

let isAuthenticated = true;
const slug = '/user';

export const handlers = [
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
		const user = umbUsersData.getById(id);

		if (!user) return res(ctx.status(404));

		return res(ctx.status(200), ctx.json(user));
	}),

	rest.put(umbracoPath(`${slug}/:id`), async (req, res, ctx) => {
		const data = await req.json();
		if (!data) return;

		const saved = umbUsersData.save(data);

		return res(ctx.status(200), ctx.json(saved));
	}),
	rest.post(umbracoPath('/user/login'), (_req, res, ctx) => {
		// Persist user's authentication in the session
		isAuthenticated = true;
		return res(
			// Respond with a 200 status code
			ctx.status(201)
		);
	}),

	rest.post(umbracoPath('/user/logout'), (_req, res, ctx) => {
		// Persist user's authentication in the session
		isAuthenticated = false;
		return res(
			// Respond with a 200 status code
			ctx.status(201)
		);
	}),

	rest.get(umbracoPath('/user'), (_req, res, ctx) => {
		// Check if the user is authenticated in this session
		if (!isAuthenticated) {
			// If not authenticated, respond with a 403 error
			return res(
				ctx.status(403),
				ctx.json({
					errorMessage: 'Not authorized',
				})
			);
		}
		// If authenticated, return a mocked user details
		return res(
			ctx.status(200),
			ctx.json({
				username: 'admin',
				role: 'administrator',
			})
		);
	}),

	rest.get(umbracoPath('/user/sections'), (_req, res, ctx) => {
		return res(
			ctx.status(200),
			ctx.json({
				sections: ['Umb.Section.Content', 'Umb.Section.Media', 'Umb.Section.Settings', 'My.Section.Custom'],
			})
		);
	}),
];
