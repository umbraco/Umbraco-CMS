import { umbLoginData } from '../data/login.data.js';
import { delay, http, type HttpHandler, HttpResponse } from 'msw';
import type { ManifestResponseModel, ResetPasswordRequestModel } from '@umbraco-cms/backoffice/external/backend-api';
import { PostSecurityForgotPasswordData } from '../../api/types.gen.js';

export const handlers: HttpHandler[] = [
	http.get<any, Array<ManifestResponseModel>>('/umbraco/management/api/v1/manifest/manifest/public', async () => {
		await delay();
		return HttpResponse.json<Array<ManifestResponseModel>>([
			{
				name: 'Test Package',
				version: '1.0.0',
				extensions: [],
			},
		]);
	}),

	http.post<any, { username: string; password: string }>(
		'/umbraco/management/api/v1/security/back-office/login',
		async ({ request }) => {
			const json = await request.json();

			const username = json.username;
			const password = json.password;

			const { status, data } = umbLoginData.login(username, password);

			return HttpResponse.json(data, { status });
		}
	),

	http.post('/umbraco/management/api/v1/security/forgot-password', async () => {
		await delay();
		return new Response('', { status: 200 });
	}),

	http.post<any, { resetCode: string }>(
		'/umbraco/management/api/v1/security/forgot-password/verify',
		async ({ request }) => {
			const json = await request.json();

			const code = json.resetCode;

			const response = umbLoginData.validatePasswordResetCode(code);

			await delay();

			return HttpResponse.json(response.data, { status: response.status });
		}
	),

	http.post<any, ResetPasswordRequestModel>('/umbraco/management/api/v1/security/forgot-password/reset', async () => {
		await delay();
		return HttpResponse.json(null, { status: 204 });
	}),

	http.post<any, { code: string }>(
		'/umbraco/management/api/v1/security/back-office/verify-2fa',
		async ({ request }) => {
			const body = await request.json();
			await delay();
			if (body.code === 'fail') {
				return HttpResponse.json({ error: 'Invalid code' }, { status: 400 });
			}
			const user = umbLoginData.users[0];
			return HttpResponse.json(user);
		}
	),
];
