import { umbLoginData } from '../data/login.data.js';
import { delay, http, HttpResponse } from 'msw';

export const handlers = [
	http.post<any, {username: string, password: string}>('backoffice/umbracoapi/authentication/postlogin', async ({ request }) => {
		const json = await request.json();

		const username = json.username;
		const password = json.password;

		const { status, data } = umbLoginData.login(username, password);

		return HttpResponse.json(data, { status });
	}),

	http.post('backoffice/umbracoapi/authentication/PostRequestPasswordReset', async () => {
    await delay();
    return new Response('', { status: 200 });
	}),

	http.post<any, {code: string}>('backoffice/umbracoapi/authentication/validatepasswordresetcode', async ({request}) => {
		const json = await request.json();

		const code = json.code;

		const { status } = umbLoginData.validatePasswordResetCode(code);

    await delay();

    return new Response('', { status });
	}),

	http.post('backoffice/umbracoapi/authentication/newpassword', async () => {
    await delay();
    return new Response('', { status: 200 });
	}),

	http.get('backoffice/umbracoapi/authentication/Get2faProviders', async () => {
    await delay();
    return HttpResponse.json(['Google', 'Lastpass']);
	}),

	http.post<any, {code: string}>('backoffice/umbracoapi/authentication/PostVerify2faCode', async ({request}) => {
		const body = await request.json();
    await delay();
		if (body.code === 'fail') {
      return HttpResponse.json({ Message: 'Invalid code' }, { status: 400 });
		}
    const user = umbLoginData.users[0];
    return HttpResponse.json(user);
	}),
];
