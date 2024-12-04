import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbCurrentUserMfaEnableModalConfig {
	providerName: string;
	displayName: string;
}

export const UMB_CURRENT_USER_MFA_ENABLE_PROVIDER_MODAL = new UmbModalToken<UmbCurrentUserMfaEnableModalConfig, never>(
	'Umb.Modal.CurrentUserMfaEnableProvider',
	{
		modal: {
			type: 'sidebar',
			size: 'small',
		},
	},
);
