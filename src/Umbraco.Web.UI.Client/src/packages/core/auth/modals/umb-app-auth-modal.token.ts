import type { UmbUserLoginState } from '../types.js';
import { UmbModalToken, UmbPersistentModalDialogElement } from '@umbraco-cms/backoffice/modal';

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
		// Persistent dialog: the auth modal only opens when there is no session (login / timeout
		// re-auth), so it must not be dismissable by ESC or backdrop click — the user has to
		// authenticate. It still closes programmatically once the session is (re)established.
		type: 'custom',
		element: UmbPersistentModalDialogElement,
	},
});
