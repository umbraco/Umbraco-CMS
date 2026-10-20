import type { UmbDocumentVariantPickerData, UmbDocumentVariantPickerValue } from '../../../modals/types.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

/**
 * @deprecated No longer used internally. Use `UMB_CONTENT_PUBLISH_MODAL_ALIAS` from `@umbraco-cms/backoffice/content` instead. Scheduled for removal in Umbraco 19.
 * No runtime warning is emitted here: this constant's module is reachable via the public `@umbraco-cms/backoffice/document` barrel, so importing the
 * package for any reason would evaluate this module and fire a false-positive warning. `UmbDocumentPublishModalElement`'s constructor warns accurately instead,
 * since it only runs when the deprecated modal is genuinely opened.
 */
export const UMB_DOCUMENT_PUBLISH_MODAL_ALIAS = 'Umb.Modal.DocumentPublish';

/** @deprecated Use `UmbContentPublishModalData` from `@umbraco-cms/backoffice/content` instead. Scheduled for removal in Umbraco 19. */
export interface UmbDocumentPublishModalData extends UmbDocumentVariantPickerData {
	headline?: string;
	confirmLabel?: string;
}

/** @deprecated Use `UmbContentPublishModalValue` from `@umbraco-cms/backoffice/content` instead. Scheduled for removal in Umbraco 19. */
// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbDocumentPublishModalValue extends UmbDocumentVariantPickerValue {}

/** @deprecated No longer used internally. Use `UMB_CONTENT_PUBLISH_MODAL` from `@umbraco-cms/backoffice/content` instead. Scheduled for removal in Umbraco 19. */
export const UMB_DOCUMENT_PUBLISH_MODAL = new UmbModalToken<UmbDocumentPublishModalData, UmbDocumentPublishModalValue>(
	UMB_DOCUMENT_PUBLISH_MODAL_ALIAS,
	{
		modal: {
			type: 'dialog',
		},
	},
);
