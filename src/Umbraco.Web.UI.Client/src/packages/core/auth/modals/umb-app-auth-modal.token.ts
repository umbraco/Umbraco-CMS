import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export type UmbModalAppAuthValue = {
	providerName?: string;
};

export const UMB_MODAL_APP_AUTH = new UmbModalToken<never, UmbModalAppAuthValue>('Umb.Modal.AppAuth', {
	modal: {
		type: 'dialog',
	},
});
