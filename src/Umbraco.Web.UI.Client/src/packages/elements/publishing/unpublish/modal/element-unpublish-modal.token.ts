import type { UmbElementVariantPickerData, UmbElementVariantPickerValue } from '../../../modals/types.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

/** @deprecated No longer used internally. Use `UMB_CONTENT_UNPUBLISH_MODAL_ALIAS` from `@umbraco-cms/backoffice/content` instead. Scheduled for removal in Umbraco 20. */
export const UMB_ELEMENT_UNPUBLISH_MODAL_ALIAS = 'Umb.Modal.ElementUnpublish';

/** @deprecated Use `UmbContentUnpublishModalData` from `@umbraco-cms/backoffice/content` instead. Scheduled for removal in Umbraco 20. */
// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbElementUnpublishModalData extends UmbElementVariantPickerData {}

/** @deprecated Use `UmbContentUnpublishModalValue` from `@umbraco-cms/backoffice/content` instead. Scheduled for removal in Umbraco 20. */
// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbElementUnpublishModalValue extends UmbElementVariantPickerValue {}

/** @deprecated No longer used internally. Use `UMB_CONTENT_UNPUBLISH_MODAL` from `@umbraco-cms/backoffice/content` instead. Scheduled for removal in Umbraco 20. */
export const UMB_ELEMENT_UNPUBLISH_MODAL = new UmbModalToken<
	UmbElementUnpublishModalData,
	UmbElementUnpublishModalValue
>(UMB_ELEMENT_UNPUBLISH_MODAL_ALIAS, {
	modal: {
		type: 'dialog',
	},
});
