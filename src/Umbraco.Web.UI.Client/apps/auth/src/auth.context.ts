import { LoginRequestModel, IUmbAuthContext } from './types';

export class UmbAuthContext implements IUmbAuthContext {
	readonly #AUTH_URL = '/umbraco/management/api/v1/security/back-office';

	async login(data: LoginRequestModel) {
		//TODO: call authUrl with data
		const request = new Request(this.#AUTH_URL + '/login', {
			method: 'POST',
			body: JSON.stringify({
				username: data.username,
				password: data.password,
			}),
			headers: {
				'Content-Type': 'application/json',
			},
		});
		const response = await fetch(request);

		return { error: response.ok ? undefined : response.statusText };
	}
}

class UmbMockAPI {
	static async loginSuccess(data: LoginRequestModel) {
		await new Promise((resolve) => setTimeout(resolve, 1000));

		return {};
	}

	static async loginFail(data: LoginRequestModel) {
		await new Promise((resolve) => setTimeout(resolve, 1000));

		return { error: 'Invalid credentials' };
	}
}
