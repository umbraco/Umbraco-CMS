import { IUmbAuthContext, LoginRequestModel, LoginResponse } from '../types.ts';

export class UmbAuthContext implements IUmbAuthContext {
	supportsPersistLogin = false;

	login(_data: LoginRequestModel): Promise<LoginResponse> {
		throw new Error('Method not implemented.');
	}
}
