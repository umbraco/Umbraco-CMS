import { LoginRequestModel, IUmbAuthContext, LoginResponse } from './types';

export class UmbAuthLegacyContext implements IUmbAuthContext {
	readonly #AUTH_URL = '/umbraco/backoffice/umbracoapi/authentication/postlogin';

	login(data: LoginRequestModel): Promise<LoginResponse> {
		throw new Error('Method not implemented.');
	}
}
