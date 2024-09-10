import type { UmbUserKindType } from '../../utils/index.js';
import { UMB_CREATE_USER_MODAL_ALIAS } from './constants.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbCreateUserModalData {
	user: {
		kind?: UmbUserKindType;
	};
}

export const UMB_CREATE_USER_MODAL = new UmbModalToken<UmbCreateUserModalData>(UMB_CREATE_USER_MODAL_ALIAS, {
	modal: {
		type: 'dialog',
		size: 'small',
	},
});
