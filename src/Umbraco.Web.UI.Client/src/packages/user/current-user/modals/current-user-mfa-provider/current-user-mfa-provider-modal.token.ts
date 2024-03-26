import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbCurrentUserMfaProviderModalConfig {
	providerName: string;
	isEnabled: boolean;
}

export interface UmbCurrentUserMfaProviderModalValue {
	secret?: string;
	code?: string;
}

export const UMB_CURRENT_USER_MFA_PROVIDER_MODAL = new UmbModalToken<
	UmbCurrentUserMfaProviderModalConfig,
	UmbCurrentUserMfaProviderModalValue
>('Umb.Modal.CurrentUserMfaProvider', {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
});
