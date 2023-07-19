import {
	IUmbAuthContext,
	LoginRequestModel,
	LoginResponse,
	NewPasswordResponse,
	ResetPasswordResponse,
	ValidatePasswordResetCodeResponse,
} from '../types.ts';

export class UmbAuthContext implements IUmbAuthContext {
	resetPassword(_username: string): Promise<ResetPasswordResponse> {
		throw new Error('Method not implemented.');
	}
	validatePasswordResetCode(_code: string): Promise<ValidatePasswordResetCodeResponse> {
		throw new Error('Method not implemented.');
	}
	newPassword(_password: string, _code: string): Promise<NewPasswordResponse> {
		throw new Error('Method not implemented.');
	}
	supportsPersistLogin = false;

	login(_data: LoginRequestModel): Promise<LoginResponse> {
		throw new Error('Method not implemented.');
	}
}
