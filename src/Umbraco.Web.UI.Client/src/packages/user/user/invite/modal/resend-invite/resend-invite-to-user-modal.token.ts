import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export type UmbResendInviteToUserModalData = {
	user: {
		unique: string;
	};
};

export type UmbResendInviteToUserModalValue = never;

export const UMB_RESEND_INVITE_TO_USER_MODAL = new UmbModalToken<
	UmbResendInviteToUserModalData,
	UmbResendInviteToUserModalValue
>('Umb.Modal.User.Invite.Resend', {
	modal: {
		type: 'dialog',
		size: 'small',
	},
});
