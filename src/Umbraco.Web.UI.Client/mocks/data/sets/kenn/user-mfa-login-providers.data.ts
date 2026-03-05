import type { UserTwoFactorProviderModel } from '@umbraco-cms/backoffice/external/backend-api';

/**
 * Mock data for MFA login providers
 */
export const data: Array<UserTwoFactorProviderModel> = [
	{
		isEnabledOnUser: false,
		providerName: 'Google Authenticator',
	},
];
