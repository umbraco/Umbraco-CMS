import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbMediaCreateOptionsModalData {
	parent: UmbEntityModel;
	mediaType: {
		unique: string;
	} | null;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
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
