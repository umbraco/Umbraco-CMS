import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

/** Data passed to the element create options modal. */
export interface UmbElementCreateOptionsModalData {
	parent: UmbEntityModel;
}

/** Value returned by the element create options modal. */
// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbElementCreateOptionsModalValue {}

/** Modal token for the element create options dialog, which presents allowed element types and create option actions. */
export const UMB_ELEMENT_CREATE_OPTIONS_MODAL = new UmbModalToken<
	UmbElementCreateOptionsModalData,
	UmbElementCreateOptionsModalValue
>('Umb.Modal.Element.CreateOptions', {
	modal: {
		type: 'dialog',
	},
});
