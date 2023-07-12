import { LoginRequestModel, IUmbAuthContext, LoginResponse } from './types.js';

export class UmbAuthLegacyContext implements IUmbAuthContext {
	readonly #AUTH_URL = '/umbraco/backoffice/umbracoapi/authentication/postlogin';
	readonly supportsPersistLogin = true;

	async login(data: LoginRequestModel): Promise<LoginResponse> {
    throw new Error('lol')
    const response = await fetch(this.#AUTH_URL, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(data),
    });

    if (response.ok) {
      return response.json();
    }

    throw new Error('Login failed');
	}
}
