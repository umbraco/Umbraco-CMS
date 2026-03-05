import type { UserTwoFactorProviderModel } from '@umbraco-cms/backoffice/external/backend-api';

/**
 * Mock data for MFA login providers
 * This is usually linked to a user, but for the sake of the mock, we're just going to have a list of providers
 */
export const data: Array<UserTwoFactorProviderModel> = [
	{
		isEnabledOnUser: true,
		providerName: 'Google Authenticator',
	},
	{
		isEnabledOnUser: false,
		providerName: 'sms',
	},
];
