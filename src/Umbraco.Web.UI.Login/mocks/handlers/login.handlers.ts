import { umbLoginData } from '../data/login.data.js';
import { rest } from 'msw';

export const handlers = [
	rest.post('http://localhost:5173/umbraco/backoffice/umbracoapi/authentication/postlogin', async (req, res, ctx) => {
		const json = await req.json();

		const username = json.username;
		const password = json.password;

		const { status, data } = umbLoginData.login(username, password);

		return res(ctx.status(status), ctx.json(data));
	}),

	rest.post('http://localhost:5173/umbraco/backoffice/umbracoapi/authentication/reset', async (req, res, ctx) => {
		return res(ctx.status(200));
	}),

	rest.post(
		'http://localhost:5173/umbraco/backoffice/umbracoapi/authentication/validatepasswordresetcode',
		async (req, res, ctx) => {
			const json = await req.json();

			const code = json.code;

			const { status } = umbLoginData.validatePasswordResetCode(code);

			return res(ctx.status(status));
		}
	),
];
