import { UMB_CREATE_USER_MODAL_ALIAS } from './constants.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export const UMB_CREATE_USER_MODAL = new UmbModalToken(UMB_CREATE_USER_MODAL_ALIAS, {
	modal: {
		type: 'dialog',
		size: 'small',
	},
});
