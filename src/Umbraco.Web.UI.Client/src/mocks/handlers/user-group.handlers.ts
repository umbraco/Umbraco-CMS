const { rest } = window.MockServiceWorker;
import { umbUserGroupsData } from '../data/user-groups.data.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

const slug = '/user-group';

export const handlers = [
	rest.get(umbracoPath(`${slug}`), (req, res, ctx) => {
		const response = umbUserGroupsData.getAll();

		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get(umbracoPath(`${slug}/:id`), (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return;
		const userGroup = umbUserGroupsData.getById(id);

		return res(ctx.status(200), ctx.json(userGroup));
	}),
];
