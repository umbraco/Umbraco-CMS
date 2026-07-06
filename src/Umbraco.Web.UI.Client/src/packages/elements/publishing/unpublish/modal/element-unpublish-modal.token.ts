import type { UmbElementVariantPickerData, UmbElementVariantPickerValue } from '../../../modals/types.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export const UMB_ELEMENT_UNPUBLISH_MODAL_ALIAS = 'Umb.Modal.ElementUnpublish';

export interface UmbElementUnpublishModalData extends UmbElementVariantPickerData {
	elementUnique?: string;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbElementUnpublishModalValue extends UmbElementVariantPickerValue {}

export const UMB_ELEMENT_UNPUBLISH_MODAL = new UmbModalToken<
	UmbElementUnpublishModalData,
	UmbElementUnpublishModalValue
>(UMB_ELEMENT_UNPUBLISH_MODAL_ALIAS, {
	modal: {
		type: 'dialog',
	},
});
