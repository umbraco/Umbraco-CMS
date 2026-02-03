import type { UmbElementVariantPickerData, UmbElementVariantPickerValue } from '../../../modals/types.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export const UMB_ELEMENT_PUBLISH_MODAL_ALIAS = 'Umb.Modal.ElementPublish';

export interface UmbElementPublishModalData extends UmbElementVariantPickerData {
	headline?: string;
	confirmLabel?: string;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbElementPublishModalValue extends UmbElementVariantPickerValue {}

export const UMB_ELEMENT_PUBLISH_MODAL = new UmbModalToken<UmbElementPublishModalData, UmbElementPublishModalValue>(
	UMB_ELEMENT_PUBLISH_MODAL_ALIAS,
	{
		modal: {
			type: 'dialog',
		},
	},
);
