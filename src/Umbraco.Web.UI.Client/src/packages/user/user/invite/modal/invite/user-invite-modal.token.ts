import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export const UMB_INVITE_USER_MODAL = new UmbModalToken<never, never>('Umb.Modal.User.Invite', {
	modal: {
		type: 'dialog',
		size: 'small',
	},
});
