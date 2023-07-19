export type LoginRequestModel = {
	username: string;
	password: string;
	persist: boolean;
};

export interface IUmbAuthContext {
	login(data: LoginRequestModel): Promise<LoginResponse>;
	resetPassword(username: string): Promise<ResetPasswordResponse>;
	validatePasswordResetCode(code: string): Promise<ValidatePasswordResetCodeResponse>;
	newPassword(password: string, code: string): Promise<NewPasswordResponse>;
	supportsPersistLogin: boolean;
	returnPath: string;
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

export type NewPasswordResponse = {
	error?: string;
	status: number;
};
