import type { UmbUserKindType } from '../../../utils/index.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbCreateUserSuccessModalData {
	user: {
		unique: string;
		kind: UmbUserKindType;
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
