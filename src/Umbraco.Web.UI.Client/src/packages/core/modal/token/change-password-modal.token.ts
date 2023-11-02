import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbChangePasswordModalData {
	userId: string;
}

export interface UmbChangePasswordModalValue {
	oldPassword: string;
	newPassword: string;
}

export const UMB_CHANGE_PASSWORD_MODAL = new UmbModalToken<UmbChangePasswordModalData, UmbChangePasswordModalValue>(
	'Umb.Modal.ChangePassword',
	{
		type: 'dialog',
	},
);
