import { LoginRequestModel, IUmbAuthContext } from './types';

export class UmbAuthContext implements IUmbAuthContext {
	readonly #AUTH_URL = '/umbraco/management/api/v1/security/back-office';

	getErrorText(response: Response) {
		switch (response.status) {
			case 401:
				return 'Oops! It seems like your login credentials are invalid or expired. Please double-check your username and password and try again.';

			default:
				return response.statusText;
		}
	}

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

		return { error: response.ok ? undefined : this.getErrorText(response) };
	}
}
