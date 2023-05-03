import { rest } from 'msw';
import { umbUserGroupsData } from '../data/user-groups.data';
import type { UserGroupDetails } from '../../../backoffice/users/user-groups/types';

export const handlers = [
	rest.get('/umbraco/backoffice/user-groups/list/items', (req, res, ctx) => {
		const items = umbUserGroupsData.getAll();

		const response = {
			total: items.length,
			items,
		};

		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get('/umbraco/backoffice/user-groups/details/:id', (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return;

		const userGroup = umbUserGroupsData.getById(id);

		return res(ctx.status(200), ctx.json(userGroup));
	}),

	rest.post<Array<UserGroupDetails>>('/umbraco/backoffice/user-groups/save', async (req, res, ctx) => {
		const data = await req.json();
		if (!data) return;

		const saved = umbUserGroupsData.save(data);
		return res(ctx.status(200), ctx.json(saved));
	}),
];
