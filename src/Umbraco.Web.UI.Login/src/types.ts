export type LoginRequestModel = {
	username: string;
	password: string;
	persist: boolean;
};

export interface IUmbAuthContext {
	login(data: LoginRequestModel): Promise<LoginResponse>;
	resetPassword(username: string): Promise<ResetPasswordResponse>;
	validatePasswordResetCode(code: string): Promise<ValidatePasswordResetCodeResponse>;
	supportsPersistLogin: boolean;
}

export type LoginResponse = {
	data?: string;
	error?: string;
	status: number;
};

export type ResetPasswordResponse = {
	error?: string;
	status: number;
};

export type ValidatePasswordResetCodeResponse = {
	error?: string;
	status: number;
};
