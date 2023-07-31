import type {
	LoginRequestModel,
	LoginResponse,
	ResetPasswordResponse,
	ValidatePasswordResetCodeResponse,
} from '../types.js';

export class UmbAuthLegacyRepository {
	readonly #authURL = 'backoffice/umbracoapi/authentication/postlogin';

	public async login(data: LoginRequestModel): Promise<LoginResponse> {
		const request = new Request(this.#authURL, {
			method: 'POST',
			body: JSON.stringify({
				username: data.username,
				password: data.password,
				rememberMe: data.persist,
			}),
			headers: {
				'Content-Type': 'application/json',
			},
		});
		const response = await fetch(request);

		return {
			status: response.status,
			error: response.ok ? undefined : this.#getErrorText(response),
		};
	}

	public async resetPassword(email: string): Promise<ResetPasswordResponse> {
		const request = new Request('backoffice/umbracoapi/authentication/PostRequestPasswordReset', {
			method: 'POST',
			body: JSON.stringify({
				email,
			}),
			headers: {
				'Content-Type': 'application/json',
			},
		});
		const response = await fetch(request);

		return {
			status: response.status,
			error: response.ok ? undefined : this.#getErrorText(response),
		};
	}

	public async validatePasswordResetCode(user: string, code: string): Promise<ValidatePasswordResetCodeResponse> {
		const request = new Request('backoffice/umbracoapi/authentication/validatepasswordresetcode', {
			method: 'POST',
			body: JSON.stringify({
				userId: user,
				resetCode: code,
			}),
			headers: {
				'Content-Type': 'application/json',
			},
		});
		const response = await fetch(request);

		return {
			status: response.status,
			error: response.ok ? undefined : this.#getErrorText(response),
		};
	}

	public async newPassword(password: string, resetCode: string, userId: number): Promise<LoginResponse> {
		const request = new Request('backoffice/umbracoapi/authentication/PostSetPassword', {
			method: 'POST',
			body: JSON.stringify({
				password,
				resetCode,
				userId,
			}),
			headers: {
				'Content-Type': 'application/json',
			},
		});
		const response = await fetch(request);

		return {
			status: response.status,
			error: response.ok ? undefined : this.#getErrorText(response),
		};
	}

	#getErrorText(response: Response) {
		switch (response.status) {
			case 400:
				return 'Oops! It seems like your login credentials are invalid or expired. Please double-check your username and password and try again.';

			case 401:
				return 'Oops! It seems like your login credentials are invalid or expired. Please double-check your username and password and try again.';

			case 500:
				return "We're sorry, but the server encountered an unexpected error. Please refresh the page or try again later..";

			default:
				return response.statusText;
		}
	}
}
