import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbMediaTypeImportModalData {
	unique: string | null;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbMediaTypeImportModalValue {}

export const UMB_MEDIA_TYPE_IMPORT_MODAL = new UmbModalToken<UmbMediaTypeImportModalData, UmbMediaTypeImportModalValue>(
	'Umb.Modal.MediaType.Import',
	{
		modal: {
			type: 'sidebar',
			size: 'small',
		},
	},
);
