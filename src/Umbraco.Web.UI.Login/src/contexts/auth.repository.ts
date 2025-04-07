import {
	LoginRequestModel,
	LoginResponse,
	MfaCodeResponse,
	NewPasswordResponse,
	PasswordConfigurationModel,
	ResetPasswordResponse,
	ValidateInviteCodeResponse,
	ValidatePasswordResetCodeResponse,
} from '../types.js';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import {
	ApiError,
	CancelError,
	ProblemDetails,
	SecurityService,
	UserService,
} from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

export class UmbAuthRepository extends UmbRepositoryBase {
	#localize = new UmbLocalizationController(this);

	public async login(data: LoginRequestModel): Promise<LoginResponse> {
		try {
			const request = new Request('management/api/v1/security/back-office/login', {
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
			const requestData = new Request('management/api/v1/security/back-office/verify-2fa', {
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
		const response = await tryExecuteAndNotify(
			this,
			SecurityService.postSecurityForgotPassword({
				requestBody: {
					email,
				},
			})
		);

		if (response.error) {
			return {
				error: this.#getApiErrorDetailText(response.error, 'Could not reset the password'),
			};
		}

		return {};
	}

	public async validatePasswordResetCode(
		userId: string,
		resetCode: string
	): Promise<ValidatePasswordResetCodeResponse> {
		const { data, error } = await tryExecuteAndNotify(
			this,
			SecurityService.postSecurityForgotPasswordVerify({
				requestBody: {
					user: {
						id: userId,
					},
					resetCode,
				},
			})
		);

		if (error || !data) {
			return {
				error: this.#getApiErrorDetailText(error, 'Could not validate the password reset code'),
			};
		}

		return {
			passwordConfiguration: (data as unknown as { passwordConfiguration: PasswordConfigurationModel })
				.passwordConfiguration, // TODO: Fix this when the API schema has been updated
		};
	}

	public async newPassword(password: string, resetCode: string, userId: string): Promise<NewPasswordResponse> {
		const response = await tryExecuteAndNotify(
			this,
			SecurityService.postSecurityForgotPasswordReset({
				requestBody: {
					password,
					resetCode,
					user: {
						id: userId,
					},
				},
			})
		);

		if (response.error) {
			return {
				error: this.#getApiErrorDetailText(response.error, 'Could not reset the password'),
			};
		}

		return {};
	}

	public async validateInviteCode(token: string, userId: string): Promise<ValidateInviteCodeResponse> {
		const { data, error } = await tryExecuteAndNotify(
			this,
			UserService.postUserInviteVerify({
				requestBody: {
					token,
					user: {
						id: userId,
					},
				},
			})
		);

		if (error || !data) {
			return {
				error: this.#getApiErrorDetailText(error, 'Could not validate the invite code'),
			};
		}

		return {
			passwordConfiguration: (data as unknown as { passwordConfiguration: PasswordConfigurationModel })
				.passwordConfiguration, // TODO: Fix this when the API schema has been updated
		};
	}

	public async newInvitedUserPassword(password: string, token: string, userId: string): Promise<NewPasswordResponse> {
		const response = await tryExecuteAndNotify(
			this,
			UserService.postUserInviteCreatePassword({
				requestBody: {
					password,
					token,
					user: {
						id: userId,
					},
				},
			})
		);

		if (response.error) {
			return {
				error: this.#getApiErrorDetailText(response.error, 'Could not create a password for the invited user'),
			};
		}

		return {};
	}

	#getApiErrorDetailText(error: ApiError | CancelError | undefined, fallbackText?: string): string | undefined {
		if (error instanceof ApiError) {
			// Try to parse the body
			return typeof error.body === 'object'
				? (error.body as ProblemDetails).title ?? fallbackText
				: fallbackText ?? 'An unknown error occurred.';
		}

		// Ignore cancel errors (user cancelled the request)
		if (error instanceof CancelError) {
			return undefined;
		}

		return fallbackText ?? 'An unknown error occurred.';
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
