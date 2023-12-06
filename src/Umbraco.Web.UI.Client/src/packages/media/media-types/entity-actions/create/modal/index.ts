import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbMediaTypeCreateOptionsModalData {
	parentKey: string | null;
}

export const UMB_MEDIA_TYPE_CREATE_OPTIONS_MODAL = new UmbModalToken<UmbMediaTypeCreateOptionsModalData>(
	'Umb.Modal.MediaTypeCreateOptions',
	{
		config: {
			type: 'sidebar',
			size: 'small',
		},
	},
);
