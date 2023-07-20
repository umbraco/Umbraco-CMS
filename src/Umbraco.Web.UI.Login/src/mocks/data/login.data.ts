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
				data: `
				<div>
					<h3>Custom Two Factor</h3>
					<p>Enter the code from your authenticator app</p>
					<uui-input></uui-input>
				</div>
				`,
				status: 402,
			};
		else
			return {
				data: user.id,
				status: 200,
			};
	}

	public validatePasswordResetCode(code: string) {
		const valid = this.resetCodes.includes(code);

		if (valid)
			return {
				data: null,
				status: 200,
			};
		else
			return {
				data: null,
				status: 404,
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
		},
		{
			id: '2',
			name: 'test',
			email: 'test@umbraco.com',
			password: 'test',
			twoFactor: false,
		},
	];
}

export const umbLoginData = new UmbLoginData();
