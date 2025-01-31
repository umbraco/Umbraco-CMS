import { UMB_CREATE_USER_CLIENT_CREDENTIAL_MODAL_ALIAS } from './manifests.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbCreateUserClientCredentialModalData {
	user: {
		unique: string;
	};
}

export interface UmbCreateUserClientCredentialModalValue {
	client: {
		unique: string;
		secret: string;
	};
}

export const UMB_CREATE_USER_CLIENT_CREDENTIAL_MODAL = new UmbModalToken<
	UmbCreateUserClientCredentialModalData,
	UmbCreateUserClientCredentialModalValue
>(UMB_CREATE_USER_CLIENT_CREDENTIAL_MODAL_ALIAS, {
	modal: {
		type: 'dialog',
		size: 'small',
	},
});
