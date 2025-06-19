import { UmbModalToken } from '../../modal/token/index.js';
import type { UmbUserLoginState } from '../types.js';

export type UmbModalAppAuthConfig = {
	userLoginState: UmbUserLoginState;
};

export type UmbModalAppAuthValue = {
	/**
	 * An indicator of whether the authentication was successful.
	 * @required
	 */
	success?: boolean;
};

export const UMB_MODAL_APP_AUTH = new UmbModalToken<UmbModalAppAuthConfig, UmbModalAppAuthValue>('Umb.Modal.AppAuth', {
	modal: {
		type: 'dialog',
	},
});
