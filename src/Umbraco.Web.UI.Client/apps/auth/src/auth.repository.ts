import { LoginRequestModel } from './types';

export class UmbAuthRepository {
	#authURL = '';

	constructor(authUrl: string) {
		this.#authURL = authUrl;
	}

	public async login(data: LoginRequestModel) {
		const request = new Request(this.#authURL + '/login', {
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

		//TODO: What kind of data does the old backoffice expect?
		//NOTE: this conditionally adds error and data to the response object
		return {
			status: response.status,
			...(!response.ok && { error: this.#getErrorText(response) }),
			...(response.ok && { data: 'WHAT DATA SHOULD BE RETURNED?' }),
		};
	}

	#getErrorText(response: Response) {
		switch (response.status) {
			case 401:
				return 'Oops! It seems like your login credentials are invalid or expired. Please double-check your username and password and try again.';

			case 500:
				return "We're sorry, but the server encountered an unexpected error. Please refresh the page or try again later..";

			default:
				return response.statusText;
		}
	}
}
