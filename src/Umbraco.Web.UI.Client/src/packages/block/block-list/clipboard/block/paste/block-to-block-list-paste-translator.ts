import { UMB_BLOCK_LIST_PROPERTY_EDITOR_SCHEMA_ALIAS } from '../../../property-editors/block-list-editor/constants.js';
import type { UmbBlockListValueModel } from '../../../types.js';
import type { UmbBlockClipboardEntryValueModel } from '@umbraco-cms/backoffice/block';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbClipboardPastePropertyValueTranslator } from '@umbraco-cms/backoffice/clipboard';

export class UmbBlockToBlockListClipboardPastePropertyValueTranslator
	extends UmbControllerBase
	implements UmbClipboardPastePropertyValueTranslator<UmbBlockClipboardEntryValueModel, UmbBlockListValueModel>
{
	async isCompatible(
		value: UmbBlockClipboardEntryValueModel,
		config: Array<{ alias: string; value: Array<{ contentElementTypeKey: string }> }>,
	): Promise<boolean> {
		const allowedBlockContentTypes =
			config.find((c) => c.alias === 'blocks')?.value.map((b) => b.contentElementTypeKey) ?? [];
		const blockContentTypes = value.contentData.map((c) => c.contentTypeKey);
		return blockContentTypes?.every((b) => allowedBlockContentTypes.includes(b)) ?? false;
	}

	async translate(value: UmbBlockClipboardEntryValueModel) {
		if (!value) {
			throw new Error('Value is missing.');
		}

		const valueClone = structuredClone(value);

		const blockListPropertyValue: UmbBlockListValueModel = {
			contentData: valueClone.contentData,
			settingsData: valueClone.settingsData,
			expose: [],
			layout: {
				[UMB_BLOCK_LIST_PROPERTY_EDITOR_SCHEMA_ALIAS]: valueClone.layout ?? undefined,
			},
		};

		return blockListPropertyValue;
	}
}

export { UmbBlockToBlockListClipboardPastePropertyValueTranslator as api };
