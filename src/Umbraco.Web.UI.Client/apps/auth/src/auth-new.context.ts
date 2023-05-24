import { LoginRequestModel, UmbAuthContext } from './types';

export class UmbAuthNewContext implements UmbAuthContext {
	authUrl: string;

	constructor(authUrl: string) {
		this.authUrl = authUrl;
	}

	async login(data: LoginRequestModel) {
		//TODO: call authUrl with data
		const { error } = await UmbMockAPI.login(data, false);

		//TODO Should the redirect be done here? or in the login element?

		return { error };
	}
}

class UmbMockAPI {
	static async login(data: LoginRequestModel, shouldFail = false) {
		await new Promise((resolve) => setTimeout(resolve, 1000));

		return shouldFail ? { error: 'Invalid credentials' } : {};
	}
}
