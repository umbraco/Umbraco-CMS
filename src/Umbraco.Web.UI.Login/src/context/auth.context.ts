import {
	IUmbAuthContext,
	LoginRequestModel,
	LoginResponse,
	NewPasswordResponse,
	ResetPasswordResponse,
	ValidatePasswordResetCodeResponse,
} from '../types.ts';

export class UmbAuthContext implements IUmbAuthContext {
	supportsPersistLogin = false;
	returnPath = '';
  allowPasswordReset = false;
  usernameIsEmail = true;

	resetPassword(_username: string): Promise<ResetPasswordResponse> {
		throw new Error('Method not implemented.');
	}
	validatePasswordResetCode(_code: string): Promise<ValidatePasswordResetCodeResponse> {
		throw new Error('Method not implemented.');
	}
	newPassword(_password: string, _code: string): Promise<NewPasswordResponse> {
		throw new Error('Method not implemented.');
	}

	login(_data: LoginRequestModel): Promise<LoginResponse> {
		throw new Error('Method not implemented.');
	}
}
