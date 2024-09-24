import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbChangePasswordModalData {
	user: {
		unique: string;
	};
}

export interface UmbChangePasswordModalValue {
	oldPassword: string;
	newPassword: string;
}

export const UMB_CHANGE_PASSWORD_MODAL = new UmbModalToken<UmbChangePasswordModalData, UmbChangePasswordModalValue>(
	'Umb.Modal.ChangePassword',
	{
		modal: {
			type: 'dialog',
		},
	},
);
