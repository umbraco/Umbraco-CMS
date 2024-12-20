import { UMB_BLOCK_LIST_PROPERTY_EDITOR_SCHEMA_ALIAS } from '../../property-editors/block-list-editor/constants.js';
import type { UmbBlockListValueModel } from '../../types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbClipboardEntryValuesType, UmbClipboardPasteTranslator } from '@umbraco-cms/backoffice/clipboard';

export class UmbBlockListClipboardPasteTranslator
	extends UmbControllerBase
	implements UmbClipboardPasteTranslator<UmbBlockListValueModel>
{
	async translate(values: UmbClipboardEntryValuesType) {
		if (!values.length) {
			throw new Error('Values are missing.');
		}

		const blockTypeValue = values.find((x) => x.type === 'block');

		if (!blockTypeValue) {
			throw new Error('Block value is missing.');
		}

		const valueClone = structuredClone(blockTypeValue.value);

		const blockListPropertyValue: UmbBlockListValueModel = {
			contentData: valueClone.contentData,
			settingsData: valueClone.settingsData,
			expose: valueClone.expose,
			layout: {
				[UMB_BLOCK_LIST_PROPERTY_EDITOR_SCHEMA_ALIAS]: valueClone.layout ?? undefined,
			},
		};

		return [blockListPropertyValue];
	}
}

export { UmbBlockListClipboardPasteTranslator as api };
