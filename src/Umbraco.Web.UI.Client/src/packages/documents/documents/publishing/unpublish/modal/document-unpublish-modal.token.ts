import type { UmbDocumentVariantPickerData, UmbDocumentVariantPickerValue } from '../../../modals/types.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

/** @deprecated No longer used internally. Use `UMB_CONTENT_UNPUBLISH_MODAL_ALIAS` from `@umbraco-cms/backoffice/content` instead. Scheduled for removal in Umbraco 19. */
export const UMB_DOCUMENT_UNPUBLISH_MODAL_ALIAS = 'Umb.Modal.DocumentUnpublish';

/** @deprecated Use `UmbContentUnpublishModalData` from `@umbraco-cms/backoffice/content` instead. Scheduled for removal in Umbraco 19. */
export interface UmbDocumentUnpublishModalData extends UmbDocumentVariantPickerData {
	documentUnique?: string;
}

/** @deprecated Use `UmbContentUnpublishModalValue` from `@umbraco-cms/backoffice/content` instead. Scheduled for removal in Umbraco 19. */
// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbDocumentUnpublishModalValue extends UmbDocumentVariantPickerValue {}

/** @deprecated No longer used internally. Use `UMB_CONTENT_UNPUBLISH_MODAL` from `@umbraco-cms/backoffice/content` instead. Scheduled for removal in Umbraco 19. */
export const UMB_DOCUMENT_UNPUBLISH_MODAL = new UmbModalToken<
	UmbDocumentUnpublishModalData,
	UmbDocumentUnpublishModalValue
>(UMB_DOCUMENT_UNPUBLISH_MODAL_ALIAS, {
	modal: {
		type: 'dialog',
	},
});
