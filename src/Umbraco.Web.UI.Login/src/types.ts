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
	status: number;
};

export type ValidatePasswordResetCodeResponse = {
	error?: string;
	status: number;
  data?: {
    passwordConfiguration: PasswordConfigurationModel;
  }
};

export type NewPasswordResponse = {
	error?: string;
	status: number;
};

export type ValidateInviteCodeResponse = {
  error?: string;
  status: number;
  data?: {
    passwordConfiguration: PasswordConfigurationModel;
  }
};

export type PasswordConfigurationModel = {
  minimumPasswordLength: number;
  requireNonLetterOrDigit: boolean;
  requireDigit: boolean;
  requireLowercase: boolean;
  requireUppercase: boolean;
}
