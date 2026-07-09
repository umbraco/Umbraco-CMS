import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export type UmbModalAuthTimeoutConfig = {
	remainingTimeInSeconds: number;
	onLogout: () => void;
	onContinue: () => void;
	/** Called when the countdown reaches zero without any user interaction. */
	onExpired: () => void;
};

export const UMB_MODAL_AUTH_TIMEOUT = new UmbModalToken<UmbModalAuthTimeoutConfig, never>('Umb.Modal.AuthTimeout', {
	modal: {
		type: 'dialog',
	},
});
