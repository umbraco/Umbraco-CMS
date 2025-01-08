import { UMB_BLOCK_LIST_PROPERTY_EDITOR_SCHEMA_ALIAS } from '../../../property-editors/block-list-editor/constants.js';
import type { UmbBlockListValueModel } from '../../../types.js';
import type { UmbBlockClipboardEntryValueModel } from '@umbraco-cms/backoffice/block';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbClipboardPasteTranslator } from '@umbraco-cms/backoffice/clipboard';

export class UmbBlockToBlockListClipboardPasteTranslator
	extends UmbControllerBase
	implements UmbClipboardPasteTranslator<UmbBlockClipboardEntryValueModel, UmbBlockListValueModel>
{
	async translate(value: UmbBlockClipboardEntryValueModel) {
		if (!value) {
			throw new Error('Value is missing.');
		}

		const valueClone = structuredClone(value);

		const blockListPropertyValue: UmbBlockListValueModel = {
			contentData: valueClone.contentData,
			settingsData: valueClone.settingsData,
			expose: valueClone.expose,
			layout: {
				[UMB_BLOCK_LIST_PROPERTY_EDITOR_SCHEMA_ALIAS]: valueClone.layout ?? undefined,
			},
		};

		return blockListPropertyValue;
	}
}

export { UmbBlockToBlockListClipboardPasteTranslator as api };
