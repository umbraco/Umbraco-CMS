import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbCurrentUserMfaEnableProviderModalConfig {
	providerName: string;
}

export const UMB_CURRENT_USER_MFA_ENABLE_PROVIDER_MODAL = new UmbModalToken<
	UmbCurrentUserMfaEnableProviderModalConfig,
	never
>('Umb.Modal.CurrentUserMfaProvider', {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
});
