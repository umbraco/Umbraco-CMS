import type {
	LoginRequestModel,
	LoginResponse,
	ResetPasswordResponse,
	ValidatePasswordResetCodeResponse,
} from '../types.js';

export class UmbAuthLegacyRepository {
	readonly #authURL = '/umbraco/backoffice/umbracoapi/authentication/postlogin';

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

	public async resetPassword(username: string): Promise<ResetPasswordResponse> {
		const request = new Request('/umbraco/backoffice/umbracoapi/authentication/PostRequestPasswordReset', {
			method: 'POST',
			body: JSON.stringify({
				username,
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

	public async validatePasswordResetCode(code: string): Promise<ValidatePasswordResetCodeResponse> {
		const request = new Request('/umbraco/backoffice/umbracoapi/authentication/validatepasswordresetcode', {
			method: 'POST',
			body: JSON.stringify({
				code,
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

	public async newPassword(password: string, code: string): Promise<LoginResponse> {
		const request = new Request('/umbraco/backoffice/umbracoapi/authentication/newpassword', {
			method: 'POST',
			body: JSON.stringify({
				password,
				code,
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
