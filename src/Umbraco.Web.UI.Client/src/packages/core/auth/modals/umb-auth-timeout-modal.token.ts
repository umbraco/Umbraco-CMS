import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

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
