import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbCreateUserSuccessModalData {
	userId: string;
	initialPassword: string;
}

export type UmbCreateUserSuccessModalValue = undefined;

export const UMB_CREATE_USER_SUCCESS_MODAL = new UmbModalToken<
	UmbCreateUserSuccessModalData,
	UmbCreateUserSuccessModalValue
>('Umb.Modal.User.CreateSuccess', {
	config: {
		type: 'dialog',
		size: 'small',
	},
});
