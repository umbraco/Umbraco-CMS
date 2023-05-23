import { LoginRequestModel, UmbAuthContext } from './types';

export class UmbAuthLegacyContext implements UmbAuthContext {
	returnUrl: string;
	authUrl: string;

	constructor(authUrl: string, returnUrl: string) {
		this.returnUrl = returnUrl;
		this.authUrl = authUrl;
	}

	login(data: LoginRequestModel): Promise<{ error?: string | undefined }> {
		throw new Error('Method not implemented.');
	}
}
