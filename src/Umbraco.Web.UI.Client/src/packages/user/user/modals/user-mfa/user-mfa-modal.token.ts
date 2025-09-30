import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export type UmbUserMfaModalConfiguration = {
	unique: string;
};

export const UMB_USER_MFA_MODAL = new UmbModalToken<UmbUserMfaModalConfiguration, never>('Umb.Modal.User.Mfa', {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
});
