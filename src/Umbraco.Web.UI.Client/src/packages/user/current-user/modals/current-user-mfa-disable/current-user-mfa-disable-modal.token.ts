import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbCurrentUserMfaDisableModalConfig {
	providerName: string;
	displayName: string;
}

export const UMB_CURRENT_USER_MFA_DISABLE_PROVIDER_MODAL = new UmbModalToken<
	UmbCurrentUserMfaDisableModalConfig,
	never
>('Umb.Modal.CurrentUserMfaDisableProvider', {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
});
