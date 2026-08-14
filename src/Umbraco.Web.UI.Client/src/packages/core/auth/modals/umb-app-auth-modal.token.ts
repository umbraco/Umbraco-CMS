import type { UmbUserLoginState } from '../types.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export type UmbModalAppAuthConfig = {
	userLoginState: UmbUserLoginState;
};

export type UmbModalAppAuthValue = {
	/**
	 * An indicator of whether the authentication was successful.
	 */
	success?: boolean;
};

export const UMB_MODAL_APP_AUTH = new UmbModalToken<UmbModalAppAuthConfig, UmbModalAppAuthValue>('Umb.Modal.AppAuth', {
	modal: {
		type: 'dialog',
	},
});
