import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbMediaTypeCreateOptionsModalData {
	parent: UmbEntityModel;
}

export const UMB_MEDIA_TYPE_CREATE_OPTIONS_MODAL = new UmbModalToken<UmbMediaTypeCreateOptionsModalData>(
	'Umb.Modal.MediaTypeCreateOptions',
	{
		modal: {
			type: 'sidebar',
			size: 'small',
		},
	},
);
