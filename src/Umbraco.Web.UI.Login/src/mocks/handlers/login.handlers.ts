import { umbLoginData } from '../data/login.data.js';
import { rest } from 'msw';

export const handlers = [
	rest.post('/umbraco/backoffice/umbracoapi/authentication/postlogin', async (req, res, ctx) => {
		const json = await req.json();

		const username = json.username;
		const password = json.password;

		const { status, data } = umbLoginData.login(username, password);

		return res(ctx.status(status), ctx.delay(), ctx.json(data));
	}),

	rest.post('/umbraco/backoffice/umbracoapi/authentication/reset', async (_req, res, ctx) => {
		return res(ctx.status(200), ctx.delay());
	}),

	rest.post(
		'/umbraco/backoffice/umbracoapi/authentication/validatepasswordresetcode',
		async (req, res, ctx) => {
			const json = await req.json();

			const code = json.code;

			const { status } = umbLoginData.validatePasswordResetCode(code);

			return res(ctx.status(status), ctx.delay());
		}
	),

	rest.post('/umbraco/backoffice/umbracoapi/authentication/newpassword', async (_req, res, ctx) => {
		return res(ctx.status(200), ctx.delay());
	}),
];
