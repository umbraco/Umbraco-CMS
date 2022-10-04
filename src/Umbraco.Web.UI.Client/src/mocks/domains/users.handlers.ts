import { rest } from 'msw';
import type { UserDetails } from '../../core/models';
import { umbUsersData } from '../data/users.data';

// TODO: add schema
export const handlers = [
	rest.get('/umbraco/backoffice/users', (req, res, ctx) => {
		const response = {
			items: umbUsersData.getItems('user'),
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

		return res(ctx.status(200), ctx.json(saved));
	}),
];
