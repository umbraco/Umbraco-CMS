import { LoginRequestModel, IUmbAuthContext } from './types';

export class UmbAuthContext implements IUmbAuthContext {
	readonly #AUTH_URL = '/umbraco/management/api/v1.0/security/back-office';

	async login(data: LoginRequestModel) {
		//TODO: call authUrl with data
		return UmbMockAPI.loginSuccess(data);
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
