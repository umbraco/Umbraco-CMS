export type LoginRequestModel = {
	username: string;
	password: string;
	persist: boolean;
};

export interface IUmbAuthContext {
	login(data: LoginRequestModel): Promise<LoginResponse>;
	resetPassword(username: string): Promise<ResetPasswordResponse>;
	validatePasswordResetCode(userId: string, resetCode: string): Promise<ValidatePasswordResetCodeResponse>;
	newPassword(password: string, resetCode: string, userId: string): Promise<NewPasswordResponse>;
	newInvitedUserPassword(password: string): Promise<NewPasswordResponse>;
	getMfaProviders(): Promise<MfaProvidersResponse>;
	validateMfaCode(code: string, provider: string): Promise<LoginResponse>;
	getPasswordConfig(userId: string): Promise<any>; //TODO Figure out the type
	getInvitedUser(): Promise<any>; //TODO Figure out the type
	disableLocalLogin: boolean;
	supportsPersistLogin: boolean;
	returnPath: string;
	twoFactorView?: string;
	isMfaEnabled?: boolean;
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
