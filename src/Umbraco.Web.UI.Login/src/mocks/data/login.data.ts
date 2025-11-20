class UmbLoginData {
	public login(email: string, password: string) {
		const user = this.users.find((user) => user.email === email && user.password === password);

		if (!user)
			return {
				data: null,
				status: 404,
			};
		else if (user.twoFactor)
			return {
				data: user,
				status: 402,
			};
		else
			return {
				data: user,
				status: 200,
			};
	}

	public validatePasswordResetCode(code: string) {
		const valid = this.resetCodes.includes(code);

		if (valid)
			return {
				data: {
					passwordConfiguration: {
						minimumLength: 8,
						requireDigit: true,
						requireLowercase: true,
						requireUppercase: true,
						requireNonAlphanumeric: true,
					},
				},
				status: 200,
			};
		else
			return {
				error: 'Invalid code',
				status: 400,
			};
	}

	resetCodes = ['valid', 'invalid'];

	users = [
		{
			id: '1',
			name: '2fa',
			email: '2fa@umbraco.com',
			password: '2fa',
			twoFactor: true,
			enabledTwoFactorProviderNames: ['sms'],
		},
		{
			id: '2',
			name: '2fa',
			email: '2fa@umbraco.com',
			password: 'html',
			twoFactor: true,
			twoFactorLoginView: '/src/mocks/customViews/my-custom-view.html',
			enabledTwoFactorProviderNames: ['sms', 'email'],
		},
		{
			id: '3',
			name: '2fa',
			email: '2fa@umbraco.com',
			password: 'js',
			twoFactor: true,
			twoFactorLoginView: '/src/mocks/customViews/my-custom-view.js',
			enabledTwoFactorProviderNames: ['sms', 'email'],
		},
		{
			id: '4',
			name: 'test',
			email: 'test@umbraco.com',
			password: 'test',
			twoFactor: false,
		},
	];
}

export const umbLoginData = new UmbLoginData();
