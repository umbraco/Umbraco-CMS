import type { PasswordConfigurationResponseModel } from './api/index.js';

export type LoginRequestModel = {
	username: string;
	password: string;
	persist: boolean;
};

export type LoginResponse = {
	data?: {
		username: string;
	};
	error?: string;
	status: number;
	twoFactorView?: string;
	twoFactorProviders?: string[];
};

export type MfaCodeResponse = {
	error?: string;
};

export type ResetPasswordResponse = {
	error?: string;
};

export type ValidatePasswordResetCodeResponse = {
	error?: string;
	passwordConfiguration?: PasswordConfigurationModel;
};

export type NewPasswordResponse = {
	error?: string;
};

export type ValidateInviteCodeResponse = {
	error?: string;
	passwordConfiguration?: PasswordConfigurationModel;
};

export type PasswordConfigurationModel = PasswordConfigurationResponseModel;

export type UmbProblemDetails = {
	type?: string | null;
	title?: string | null;
	status?: number | null;
	detail?: string | null;
	instance?: string | null;
	[key: string]:
		| unknown
		| (string | null)
		| (string | null)
		| (number | null)
		| (string | null)
		| (string | null)
		| undefined;
};
