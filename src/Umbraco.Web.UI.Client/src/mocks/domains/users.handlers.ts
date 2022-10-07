import { rest } from 'msw';
import type { UserDetails } from '../../core/models';
import { umbUsersData } from '../data/users.data';

// TODO: add schema
export const handlers = [
	rest.get('/umbraco/backoffice/users/list/items', (req, res, ctx) => {
		const items = umbUsersData.getItems('user');

		const response = {
			total: items.length,
			items,
		};

		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get('/umbraco/backoffice/users/:key', (req, res, ctx) => {
		const key = req.params.key as string;
		if (!key) return;

		const user = umbUsersData.getByKey(key);

		return res(ctx.status(200), ctx.json(user));
	}),

	rest.post<UserDetails[]>('/umbraco/backoffice/users/save', async (req, res, ctx) => {
		const data = await req.json();
		if (!data) return;

		const saved = umbUsersData.save(data);

		console.log('saved', saved);

		return res(ctx.status(200), ctx.json(saved));
	}),

	rest.post<Array<string>>('/umbraco/backoffice/users/enable', async (req, res, ctx) => {
		const data = await req.json();
		if (!data) return;

		const enabledKeys = umbUsersData.enable(data);

		return res(ctx.status(200), ctx.json(enabledKeys));
	}),

	rest.post<Array<string>>('/umbraco/backoffice/users/disable', async (req, res, ctx) => {
		const data = await req.json();
		if (!data) return;

		const disabledKeys = umbUsersData.disable(data);

		return res(ctx.status(200), ctx.json(disabledKeys));
	}),

	rest.post<Array<string>>('/umbraco/backoffice/users/delete', async (req, res, ctx) => {
		const data = await req.json();
		if (!data) return;

		const deletedKeys = umbUsersData.delete(data);

		return res(ctx.status(200), ctx.json(deletedKeys));
	}),
];
