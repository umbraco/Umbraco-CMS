import { UMB_BLOCK_LIST_PROPERTY_EDITOR_SCHEMA_ALIAS } from '../../property-editors/block-list-editor/constants.js';
import type { UmbBlockListValueModel } from '../../types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbClipboardPasteTranslator } from '@umbraco-cms/backoffice/clipboard';

export class UmbBlockListClipboardPasteTranslator
	extends UmbControllerBase
	implements UmbClipboardPasteTranslator<any, UmbBlockListValueModel>
{
	// TODO: add model for BlockClipboardEntryModel
	async translate(clipboardEntryValue: any) {
		if (!clipboardEntryValue) {
			throw new Error('Value is missing.');
		}

		const valueClone = structuredClone(clipboardEntryValue.value);

		const propertyValue: UmbBlockListValueModel = {
			contentData: valueClone.contentData,
			settingsData: valueClone.settingsData,
			expose: valueClone.expose,
			layout: {
				[UMB_BLOCK_LIST_PROPERTY_EDITOR_SCHEMA_ALIAS]: valueClone.layout ?? undefined,
			},
		};

		return propertyValue;
	}
}

export { UmbBlockListClipboardPasteTranslator as api };
