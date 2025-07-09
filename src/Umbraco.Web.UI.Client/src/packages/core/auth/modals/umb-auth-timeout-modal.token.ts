import { UmbModalToken } from '../../modal/token/modal-token.js';

export type UmbModalAuthTimeoutConfig = {
	remainingTimeInSeconds: number;
	onLogout: () => void;
	onContinue: () => void;
};

export const UMB_MODAL_AUTH_TIMEOUT = new UmbModalToken<UmbModalAuthTimeoutConfig, never>('Umb.Modal.AuthTimeout', {
	modal: {
		type: 'dialog',
	},
});
