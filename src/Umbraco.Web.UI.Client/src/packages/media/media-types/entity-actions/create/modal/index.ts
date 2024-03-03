import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbMediaTypeCreateOptionsModalData {
	parent: {
		unique: string | null;
		entityType: string;
	};
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
