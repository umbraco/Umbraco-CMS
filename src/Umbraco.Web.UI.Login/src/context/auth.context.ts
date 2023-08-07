import {
	IUmbAuthContext,
	LoginRequestModel,
	LoginResponse,
	MfaProvidersResponse,
	NewPasswordResponse,
	ResetPasswordResponse,
	ValidatePasswordResetCodeResponse,
} from '../types.ts';
import { Observable } from 'rxjs';

export class UmbAuthContext implements IUmbAuthContext {
	supportsPersistLogin = false;
	returnPath = '';
	allowPasswordReset = false;
	usernameIsEmail = true;
	disableLocalLogin = false;

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

	getMfaProviders(): Promise<MfaProvidersResponse> {
		throw new Error('Method not implemented.');
	}

	validateMfaCode(code: string, provider: string): Promise<LoginResponse> {
		throw new Error('Method not implemented.');
	}

	getIcons(): Observable<Record<string, string>> {
		throw new Error('Method not implemented.');
	}
}