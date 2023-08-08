import { Observable } from 'rxjs';

export type LoginRequestModel = {
	username: string;
	password: string;
	persist: boolean;
};

export interface IUmbAuthContext {
	disableLocalLogin: boolean;
	login(data: LoginRequestModel): Promise<LoginResponse>;
	resetPassword(username: string): Promise<ResetPasswordResponse>;
	validatePasswordResetCode(userId: string, resetCode: string): Promise<ValidatePasswordResetCodeResponse>;
	newPassword(password: string, resetCode: string, userId: string): Promise<NewPasswordResponse>;
	getMfaProviders(): Promise<MfaProvidersResponse>;
	validateMfaCode(code: string, provider: string): Promise<LoginResponse>;
	getIcons(): Observable<Record<string, string>>;
	supportsPersistLogin: boolean;
	returnPath: string;
  twoFactorView?: string;
}

export type LoginResponse = {
	data?: string;
	error?: string;
	status: number;
  twoFactorView?: string;
};

export type ResetPasswordResponse = {
	error?: string;
	status: number;
};

export type ValidatePasswordResetCodeResponse = {
	error?: string;
	status: number;
};

export type NewPasswordResponse = {
	error?: string;
	status: number;
};

export type MfaProvidersResponse = {
	error?: string;
	status: number;
	providers: string[];
};
