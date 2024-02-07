import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbCreateUserSuccessModalData {
	user: {
		unique: string;
	};
}

export type UmbCreateUserSuccessModalValue = undefined;

export const UMB_CREATE_USER_SUCCESS_MODAL = new UmbModalToken<
	UmbCreateUserSuccessModalData,
	UmbCreateUserSuccessModalValue
>('Umb.Modal.User.CreateSuccess', {
	modal: {
		type: 'dialog',
		size: 'small',
	},
});
