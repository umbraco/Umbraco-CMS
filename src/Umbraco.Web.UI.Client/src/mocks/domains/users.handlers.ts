import { rest } from 'msw';
import { v4 as uuidv4 } from 'uuid';
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

	rest.post<UserDetails[]>('/umbraco/backoffice/users/invite', async (req, res, ctx) => {
		const data = await req.json();
		if (!data) return;

		const newUser: UserDetails = {
			key: uuidv4(),
			name: data.name,
			email: data.email,
			status: 'invited',
			language: 'en',
			updateDate: new Date().toISOString(),
			createDate: new Date().toISOString(),
			failedLoginAttempts: 0,
			parentKey: '',
			isTrashed: false,
			hasChildren: false,
			type: 'user',
			icon: 'umb:icon-user',
			userGroup: data.userGroups[0],
		};

		const invited = umbUsersData.save([newUser]);

		console.log('invited', invited);

		return res(ctx.status(200), ctx.json(invited));
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
