import type { UmbElementVariantPickerData, UmbElementVariantPickerValue } from '../../../modals/types.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

/**
 * @deprecated No longer used internally. Use `UMB_CONTENT_PUBLISH_MODAL_ALIAS` from `@umbraco-cms/backoffice/content` instead. Scheduled for removal in Umbraco 20.
 * No runtime warning is emitted here: this constant's module is reachable via the public `@umbraco-cms/backoffice/element` barrel, so importing the
 * package for any reason would evaluate this module and fire a false-positive warning. `UmbElementPublishModalElement`'s constructor warns accurately instead,
 * since it only runs when the deprecated modal is genuinely opened.
 */
export const UMB_ELEMENT_PUBLISH_MODAL_ALIAS = 'Umb.Modal.ElementPublish';

/** @deprecated Use `UmbContentPublishModalData` from `@umbraco-cms/backoffice/content` instead. Scheduled for removal in Umbraco 20. */
export interface UmbElementPublishModalData extends UmbElementVariantPickerData {
	headline?: string;
	confirmLabel?: string;
	unique?: string;
}

/** @deprecated Use `UmbContentPublishModalValue` from `@umbraco-cms/backoffice/content` instead. Scheduled for removal in Umbraco 20. */
// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbElementPublishModalValue extends UmbElementVariantPickerValue {}

/** @deprecated No longer used internally. Use `UMB_CONTENT_PUBLISH_MODAL` from `@umbraco-cms/backoffice/content` instead. Scheduled for removal in Umbraco 20. */
export const UMB_ELEMENT_PUBLISH_MODAL = new UmbModalToken<UmbElementPublishModalData, UmbElementPublishModalValue>(
	UMB_ELEMENT_PUBLISH_MODAL_ALIAS,
	{
		modal: {
			type: 'dialog',
		},
	},
);
