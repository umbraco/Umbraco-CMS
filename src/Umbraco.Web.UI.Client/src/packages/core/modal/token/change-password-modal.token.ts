import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbChangePasswordModalData {
	requireOldPassword: boolean;
}

export interface UmbChangePasswordModalValue {
	newPassword: string;
	confirmPassword: string;
	oldPassword?: string;
}

export const UMB_CHANGE_PASSWORD_MODAL = new UmbModalToken<UmbChangePasswordModalData>('Umb.Modal.ChangePassword', {
	type: 'dialog',
});
