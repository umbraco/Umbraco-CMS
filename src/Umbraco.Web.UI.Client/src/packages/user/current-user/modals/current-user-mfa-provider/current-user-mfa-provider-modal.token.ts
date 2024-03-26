import type { UmbCurrentUserRepository } from '../../repository/index.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbCurrentUserMfaProviderModalConfig {
	providerName: string;
	repository: UmbCurrentUserRepository;
}

export const UMB_CURRENT_USER_MFA_PROVIDER_MODAL = new UmbModalToken<UmbCurrentUserMfaProviderModalConfig, never>(
	'Umb.Modal.CurrentUserMfaProvider',
	{
		modal: {
			type: 'sidebar',
			size: 'small',
		},
	},
);
