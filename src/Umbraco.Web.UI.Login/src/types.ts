import type { PasswordConfigurationResponseModel } from "@umbraco-cms/backoffice/external/backend-api";

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
  status: number;
  data?: {
    passwordConfiguration: PasswordConfigurationModel;
  }
};

export type PasswordConfigurationModel = PasswordConfigurationResponseModel;
