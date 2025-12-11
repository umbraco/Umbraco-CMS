import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UMB_BLOCK_RTE_PROPERTY_EDITOR_SCHEMA_ALIAS } from '@umbraco-cms/backoffice/block-rte';
import type { UmbBlockClipboardEntryValueModel } from '@umbraco-cms/backoffice/block';
import type { UmbBlockRteValueModel } from '@umbraco-cms/backoffice/block-rte';
import type { UmbClipboardPastePropertyValueTranslator } from '@umbraco-cms/backoffice/clipboard';

export class UmbTiptapBlockToBlockRteClipboardPastePropertyValueTranslator
	extends UmbControllerBase
	implements UmbClipboardPastePropertyValueTranslator<UmbBlockClipboardEntryValueModel, UmbBlockRteValueModel>
{
	async translate(value: UmbBlockClipboardEntryValueModel): Promise<UmbBlockRteValueModel> {
		if (!value) {
			throw new Error('Value is missing.');
		}

		const valueClone = structuredClone(value);

		const blockRtePropertyValue: UmbBlockRteValueModel = {
			contentData: valueClone.contentData,
			settingsData: valueClone.settingsData,
			expose: [],
			layout: {
				[UMB_BLOCK_RTE_PROPERTY_EDITOR_SCHEMA_ALIAS]: valueClone.layout ?? undefined,
			},
		};

		return blockRtePropertyValue;
	}

	async isCompatibleValue(
		propertyValue: UmbBlockRteValueModel,
		// TODO: Replace any with the correct type.
		config: Array<{ alias: string; value: [{ contentElementTypeKey: string }] }>,
	): Promise<boolean> {
		const allowedBlockContentTypes =
			config.find((c) => c.alias === 'blocks')?.value.map((b) => b.contentElementTypeKey) ?? [];
		const blockContentTypes = propertyValue.contentData.map((c) => c.contentTypeKey);
		return blockContentTypes?.every((b) => allowedBlockContentTypes.includes(b)) ?? false;
	}
}

export { UmbTiptapBlockToBlockRteClipboardPastePropertyValueTranslator as api };
