import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbChangePasswordModalData {
	requireOldPassword: boolean;
}

export const UMB_CHANGE_PASSWORD_MODAL = new UmbModalToken<UmbChangePasswordModalData>('Umb.Modal.ChangePassword', {
	type: 'dialog',
});
