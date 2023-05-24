import { LoginRequestModel, IUmbAuthContext } from './types';

export class UmbAuthLegacyContext implements IUmbAuthContext {
	readonly #AUTH_URL = '/umbraco/backoffice/umbracoapi/authentication/postlogin';

	login(data: LoginRequestModel): Promise<{ error?: string | undefined }> {
		throw new Error('Method not implemented.');
	}
}
