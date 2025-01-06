import { UMB_BLOCK_LIST_PROPERTY_EDITOR_SCHEMA_ALIAS } from '../../property-editors/block-list-editor/constants.js';
import type { UmbBlockListValueModel } from '../../types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbClipboardEntryValueModel, UmbClipboardPasteTranslator } from '@umbraco-cms/backoffice/clipboard';

export class UmbBlockToBlockListClipboardPasteTranslator
	extends UmbControllerBase
	implements UmbClipboardPasteTranslator<UmbBlockListValueModel>
{
	async translate(model: UmbClipboardEntryValueModel) {
		if (!model) {
			throw new Error('Model is missing.');
		}

		const valueClone = structuredClone(model.value);

		debugger;

		/*
		const blockListPropertyValue: UmbBlockListValueModel = {
			contentData: valueClone.contentData,
			settingsData: valueClone.settingsData,
			expose: valueClone.expose,
			layout: {
				[UMB_BLOCK_LIST_PROPERTY_EDITOR_SCHEMA_ALIAS]: valueClone.layout ?? undefined,
			},
		};
		*/

		return undefined;
	}
}

export { UmbBlockToBlockListClipboardPasteTranslator as api };
