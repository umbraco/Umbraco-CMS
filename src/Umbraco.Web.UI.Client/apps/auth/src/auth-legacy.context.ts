import { LoginRequestModel, UmbAuthContext } from './types';

export class UmbAuthLegacyContext implements UmbAuthContext {
	authUrl: string;

	constructor(authUrl: string) {
		this.authUrl = authUrl;
	}

	login(data: LoginRequestModel): Promise<{ error?: string | undefined }> {
		throw new Error('Method not implemented.');
	}
}
