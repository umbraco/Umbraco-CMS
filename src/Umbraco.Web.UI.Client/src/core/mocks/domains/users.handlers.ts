import { rest } from 'msw';
import { v4 as uuidv4 } from 'uuid';

import { umbUsersData } from '../data/users.data';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

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

		return res(ctx.status(200), ctx.json(user));
	}),

	rest.put(umbracoPath(`${slug}/:id`), async (req, res, ctx) => {
		const data = await req.json();
		if (!data) return;

		const saved = umbUsersData.save(data);

		return res(ctx.status(200), ctx.json(saved));
	}),

	// rest.get('/umbraco/backoffice/users/getByKeys', (req, res, ctx) => {
	// 	const ids = req.url.searchParams.getAll('id');
	// 	if (ids.length === 0) return;
	// 	const users = umbUsersData.getByIds(ids);

	// 	return res(ctx.status(200), ctx.json(users));
	// }),

	// rest.post<UserDetails[]>('/umbraco/backoffice/users/save', async (req, res, ctx) => {
	// 	const data = await req.json();
	// 	if (!data) return;

	// 	const saved = umbUsersData.save(data);

	// 	console.log('saved', saved);

	// 	return res(ctx.status(200), ctx.json(saved));
	// }),

	// rest.post<UserDetails[]>('/umbraco/backoffice/users/invite', async (req, res, ctx) => {
	// 	const data = await req.json();
	// 	if (!data) return;

	// 	const newUser: UserDetails = {
	// 		id: uuidv4(),
	// 		name: data.name,
	// 		email: data.email,
	// 		status: 'invited',
	// 		language: 'en',
	// 		updateDate: new Date().toISOString(),
	// 		createDate: new Date().toISOString(),
	// 		failedLoginAttempts: 0,
	// 		parentId: '',
	// 		hasChildren: false,
	// 		type: 'user',
	// 		icon: 'umb:icon-user',
	// 		userGroups: data.userGroups,
	// 		contentStartNodes: [],
	// 		mediaStartNodes: [],
	// 	};

	// 	const invited = umbUsersData.save(newUser);

	// 	console.log('invited', invited);

	// 	return res(ctx.status(200), ctx.json(invited));
	// }),

	// rest.post<Array<string>>('/umbraco/backoffice/users/enable', async (req, res, ctx) => {
	// 	const data = await req.json();
	// 	if (!data) return;

	// 	const enabledKeys = umbUsersData.enable(data);

	// 	return res(ctx.status(200), ctx.json(enabledKeys));
	// }),

	// rest.post<Array<string>>('/umbraco/backoffice/users/disable', async (req, res, ctx) => {
	// 	const data = await req.json();
	// 	if (!data) return;

	// 	const enabledKeys = umbUsersData.disable(data);

	// 	return res(ctx.status(200), ctx.json(enabledKeys));
	// }),

	// rest.post<Array<string>>('/umbraco/backoffice/users/updateUserGroup', async (req, res, ctx) => {
	// 	const data = await req.json();
	// 	if (!data) return;

	// 	const userKeys = umbUsersData.updateUserGroup(data.userKeys, data.userGroupKey);

	// 	return res(ctx.status(200), ctx.json(userKeys));
	// }),

	// rest.post<Array<string>>('/umbraco/backoffice/users/delete', async (req, res, ctx) => {
	// 	const data = await req.json();
	// 	if (!data) return;

	// 	const deletedKeys = umbUsersData.delete(data);

	// 	return res(ctx.status(200), ctx.json(deletedKeys));
	// }),
];
