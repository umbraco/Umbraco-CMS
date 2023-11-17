import { umbLoginData } from '../data/login.data.js';
import { rest } from 'msw';

export const handlers = [
	rest.post('backoffice/umbracoapi/authentication/postlogin', async (req, res, ctx) => {
		const json = await req.json();

		const username = json.username;
		const password = json.password;

		const { status, data } = umbLoginData.login(username, password);

		return res(ctx.delay(), ctx.status(status), ctx.json(data));
	}),

	rest.post('backoffice/umbracoapi/authentication/PostRequestPasswordReset', async (_req, res, ctx) => {
		return res(ctx.delay(), ctx.status(200));
	}),

	rest.post('backoffice/umbracoapi/authentication/validatepasswordresetcode', async (req, res, ctx) => {
		const json = await req.json();

		const code = json.code;

		const { status } = umbLoginData.validatePasswordResetCode(code);

		return res(ctx.delay(), ctx.status(status));
	}),

	rest.post('backoffice/umbracoapi/authentication/newpassword', async (_req, res, ctx) => {
		return res(ctx.delay(), ctx.status(200));
	}),

	rest.get('backoffice/umbracoapi/authentication/Get2faProviders', async (_req, res, ctx) => {
		return res(ctx.delay(), ctx.status(200), ctx.json(['Google', 'Lastpass']));
	}),

	rest.post('backoffice/umbracoapi/authentication/PostVerify2faCode', async (req, res, ctx) => {
		const body = await req.json();
		if (body.code === 'fail') {
			return res(ctx.delay(), ctx.status(400), ctx.json({ Message: 'Invalid code' }));
		}
    const user = umbLoginData.users[0];
		return res(ctx.delay(), ctx.status(200), ctx.json(user));
	}),
];
