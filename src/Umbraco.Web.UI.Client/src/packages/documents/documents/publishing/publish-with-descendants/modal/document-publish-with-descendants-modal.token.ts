import type { UmbDocumentVariantPickerData, UmbDocumentVariantPickerValue } from '../../../types.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export const UMB_DOCUMENT_PUBLISH_WITH_DESCENDANTS_MODAL_ALIAS = 'Umb.Modal.DocumentPublishWithDescendants';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbDocumentPublishWithDescendantsModalData extends UmbDocumentVariantPickerData {}

export interface UmbDocumentPublishWithDescendantsModalValue extends UmbDocumentVariantPickerValue {
	includeUnpublishedDescendants?: boolean;
	forceRepublish?: boolean;
}

export const UMB_DOCUMENT_PUBLISH_WITH_DESCENDANTS_MODAL = new UmbModalToken<
	UmbDocumentPublishWithDescendantsModalData,
	UmbDocumentPublishWithDescendantsModalValue
>(UMB_DOCUMENT_PUBLISH_WITH_DESCENDANTS_MODAL_ALIAS, {
	modal: {
		type: 'dialog',
	},
});
