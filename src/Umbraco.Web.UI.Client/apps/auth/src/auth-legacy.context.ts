import { LoginRequestModel, IUmbAuthContext, LoginResponse } from './types.js';

export class UmbAuthLegacyContext implements IUmbAuthContext {
	readonly #AUTH_URL = '/umbraco/backoffice/umbracoapi/authentication/postlogin';
	readonly supportsPersistLogin = true;

	login(data: LoginRequestModel): Promise<LoginResponse> {
		throw new Error('Method not implemented.');
	}
}
