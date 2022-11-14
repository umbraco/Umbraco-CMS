import { rest } from 'msw';
import { umbUserGroupsData } from '../data/user-groups.data';
import type { UserGroupDetails } from '@umbraco-cms/models';

export const handlers = [
	rest.get('/umbraco/backoffice/user-groups/list/items', (req, res, ctx) => {
		const items = umbUserGroupsData.getItems('userGroup');

		const response = {
			total: items.length,
			items,
		};

		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get('/umbraco/backoffice/user-groups/details/:key', (req, res, ctx) => {
		const key = req.params.key as string;
		if (!key) return;

		const userGroup = umbUserGroupsData.getByKey(key);

		return res(ctx.status(200), ctx.json(userGroup));
	}),

	rest.get('/umbraco/backoffice/user-groups/getByKeys', (req, res, ctx) => {
		const keys = req.url.searchParams.getAll('key');
		if (keys.length === 0) return;
		const userGroups = umbUserGroupsData.getByKeys(keys);

		return res(ctx.status(200), ctx.json(userGroups));
	}),

	rest.post<Array<UserGroupDetails>>('/umbraco/backoffice/user-groups/save', async (req, res, ctx) => {
		const data = await req.json();
		if (!data) return;

		const saved = umbUserGroupsData.save(data);
		return res(ctx.status(200), ctx.json(saved));
	}),
];
