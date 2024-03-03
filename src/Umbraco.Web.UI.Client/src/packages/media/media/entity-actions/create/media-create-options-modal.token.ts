import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbMediaCreateOptionsModalData {
	parent: {
		unique: string | null;
		entityType: string;
	};
	mediaType: {
		unique: string;
	};
}

export interface UmbMediaCreateOptionsModalValue {}

export const UMB_MEDIA_CREATE_OPTIONS_MODAL = new UmbModalToken<
	UmbMediaCreateOptionsModalData,
	UmbMediaCreateOptionsModalValue
>('Umb.Modal.Media.CreateOptions', {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
});
