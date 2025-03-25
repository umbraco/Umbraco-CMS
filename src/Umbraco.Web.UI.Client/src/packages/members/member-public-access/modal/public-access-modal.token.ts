import { UmbModalToken } from '@umbraco-cms/backoffice/modal';
import type { UmbPublicAccessModalData, UmbPublicAccessModalValue } from './types.js';

export const UMB_PUBLIC_ACCESS_MODAL = new UmbModalToken<UmbPublicAccessModalData, UmbPublicAccessModalValue>(
	'Umb.Modal.PublicAccess',
	{
		modal: {
			type: 'sidebar',
			size: 'medium',
		},
	},
);
