import {
	IUmbAuthContext,
	LoginRequestModel,
	LoginResponse,
	NewPasswordResponse,
	ResetPasswordResponse,
	ValidatePasswordResetCodeResponse,
} from '../types.ts';

export class UmbAuthContext implements IUmbAuthContext {
	resetPassword(username: string): Promise<ResetPasswordResponse> {
		throw new Error('Method not implemented.');
	}
	validatePasswordResetCode(code: string): Promise<ValidatePasswordResetCodeResponse> {
		throw new Error('Method not implemented.');
	}
	newPassword(password: string, code: string): Promise<NewPasswordResponse> {
		throw new Error('Method not implemented.');
	}
	supportsPersistLogin = false;

	login(_data: LoginRequestModel): Promise<LoginResponse> {
		throw new Error('Method not implemented.');
	}
}
