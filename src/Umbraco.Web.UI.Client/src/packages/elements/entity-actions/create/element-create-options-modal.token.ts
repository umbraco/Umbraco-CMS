import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbElementCreateOptionsModalData {
	parent: UmbEntityModel;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbElementCreateOptionsModalValue {}

export const UMB_ELEMENT_CREATE_OPTIONS_MODAL = new UmbModalToken<
	UmbElementCreateOptionsModalData,
	UmbElementCreateOptionsModalValue
>('Umb.Modal.Element.CreateOptions', {
	modal: {
		type: 'dialog',
	},
});
