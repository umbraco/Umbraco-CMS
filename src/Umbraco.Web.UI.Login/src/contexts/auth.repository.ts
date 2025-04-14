import {
	LoginRequestModel,
	LoginResponse,
	MfaCodeResponse,
	NewPasswordResponse,
	ResetPasswordResponse,
	ValidateInviteCodeResponse,
	ValidatePasswordResetCodeResponse,
} from '../types.js';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import {
	postSecurityForgotPassword,
	postSecurityForgotPasswordReset,
	postSecurityForgotPasswordVerify,
	postUserInviteCreatePassword,
	postUserInviteVerify,
} from '../api/index.js';
import { isProblemDetails } from '../utils/is-problem-details.function.js';

export class UmbAuthRepository extends UmbRepositoryBase {
	#localize = new UmbLocalizationController(this);

	public async login(data: LoginRequestModel): Promise<LoginResponse> {
		try {
			const request = new Request('/umbraco/management/api/v1/security/back-office/login', {
				method: 'POST',
				body: JSON.stringify({
					username: data.username,
					password: data.password,
				}),
				headers: {
					'Content-Type': 'application/json',
				},
			});

			const response = await fetch(request);

			if (!response.ok) {
				// If the response code is 402, it means that the user has enabled 2-factor authentication
				if (response.status === 402) {
					const responseData = await response.json();
					return {
						status: response.status,
						twoFactorView: responseData.twoFactorLoginView ?? '',
						twoFactorProviders: responseData.enabledTwoFactorProviderNames ?? [],
					};
				}

				return {
					status: response.status,
					error: await this.#getErrorText(response),
				};
			}

			return {
				status: response.status,
				data: {
					username: data.username,
				},
			};
		} catch (error) {
			return {
				status: 500,
				error: error instanceof Error ? error.message : this.#localize.term('auth_receivedErrorFromServer'),
			};
		}
	}

	public async validateMfaCode(code: string, provider: string): Promise<MfaCodeResponse> {
		try {
			const requestData = new Request('/umbraco/management/api/v1/security/back-office/verify-2fa', {
				method: 'POST',
				body: JSON.stringify({
					code,
					provider,
				}),
				headers: {
					'Content-Type': 'application/json',
				},
			});

			const response = await fetch(requestData);

			if (!response.ok) {
				return {
					error:
						response.status === 400 ? this.#localize.term('auth_mfaInvalidCode') : await this.#getErrorText(response),
				};
			}

			return {};
		} catch (error) {
			return {
				error: error instanceof Error ? error.message : this.#localize.term('auth_receivedErrorFromServer'),
			};
		}
	}

	public async resetPassword(email: string): Promise<ResetPasswordResponse> {
		const { error } = await postSecurityForgotPassword({
			body: {
				email,
			},
		});

		if (error) {
			return {
				error: this.#getApiErrorDetailText(error, 'Could not reset the password'),
			};
		}

		return {};
	}

	public async validatePasswordResetCode(
		userId: string,
		resetCode: string
	): Promise<ValidatePasswordResetCodeResponse> {
		const { data, error } = await postSecurityForgotPasswordVerify({
			body: {
				user: {
					id: userId,
				},
				resetCode,
			},
		});

		if (error || !data) {
			return {
				error: this.#getApiErrorDetailText(error, 'Could not validate the password reset code'),
			};
		}

		return data;
	}

	public async newPassword(password: string, resetCode: string, userId: string): Promise<NewPasswordResponse> {
		const { error } = await postSecurityForgotPasswordReset({
			body: {
				password,
				resetCode,
				user: {
					id: userId,
				},
			},
		});

		if (error) {
			return {
				error: this.#getApiErrorDetailText(error, 'Could not reset the password'),
			};
		}

		return {};
	}

	public async validateInviteCode(token: string, userId: string): Promise<ValidateInviteCodeResponse> {
		const { data, error } = await postUserInviteVerify({
			body: {
				token,
				user: {
					id: userId,
				},
			},
		});

		if (error || !data) {
			return {
				error: this.#getApiErrorDetailText(error, 'Could not validate the invite code'),
			};
		}

		return data;
	}

	public async newInvitedUserPassword(password: string, token: string, userId: string): Promise<NewPasswordResponse> {
		const { error } = await postUserInviteCreatePassword({
			body: {
				password,
				token,
				user: {
					id: userId,
				},
			},
		});

		if (error) {
			return {
				error: this.#getApiErrorDetailText(error, 'Could not create a password for the invited user'),
			};
		}

		return {};
	}

	#getApiErrorDetailText(error: unknown, fallbackText?: string): string | undefined {
		if (isProblemDetails(error)) {
			return error.detail ?? error.title ?? undefined;
		}

		if (false === error instanceof Error) {
			return fallbackText ?? 'An unknown error occurred.';
		}

		// Ignore cancel errors (user cancelled the request)
		if (error.name === 'CancelError') {
			return undefined;
		}

		return error.message;
	}

	async #getErrorText(response: Response): Promise<string> {
		switch (response.status) {
			case 400:
			case 401:
				return this.#localize.term('auth_userFailedLogin');

			case 402:
				return this.#localize.term('auth_mfaText');

			case 403:
				return this.#localize.term('auth_userLockedOut');

			default:
				return this.#localize.term('auth_receivedErrorFromServer');
		}
	}
}
