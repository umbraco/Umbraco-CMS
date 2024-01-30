import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbPublicAccessModalData {
	unique: string;
}

export interface UmbPublicAccessModalValue {}

export const UMB_PUBLIC_ACCESS_MODAL = new UmbModalToken<UmbPublicAccessModalData, UmbPublicAccessModalValue>(
	'Umb.Modal.PublicAccess',
	{
		modal: {
			type: 'sidebar',
			size: 'medium',
		},
	},
);
