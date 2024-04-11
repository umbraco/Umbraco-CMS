import { UmbModalToken } from '../../modal/token/index.js';
import type { UmbUserLoginState } from '../types.js';

export type UmbModalAppAuthConfig = {
	userLoginState: UmbUserLoginState;
};

export type UmbModalAppAuthValue = {
	/**
	 * The name of the provider that the user has selected to authenticate with.
	 * @required
	 */
	providerName?: string;

	/**
	 * The login hint that the user has provided to the provider.
	 * @optional
	 */
	loginHint?: string;
};

export const UMB_MODAL_APP_AUTH = new UmbModalToken<UmbModalAppAuthConfig, UmbModalAppAuthValue>('Umb.Modal.AppAuth', {
	modal: {
		type: 'dialog',
	},
});
