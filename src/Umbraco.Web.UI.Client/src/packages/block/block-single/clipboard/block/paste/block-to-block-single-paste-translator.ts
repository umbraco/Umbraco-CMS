import { UMB_BLOCK_SINGLE_PROPERTY_EDITOR_SCHEMA_ALIAS } from '../../../property-editors/block-single-editor/constants.js';
import type { UmbBlockSingleValueModel } from '../../../types.js';
import type { UmbBlockClipboardEntryValueModel } from '@umbraco-cms/backoffice/block';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbClipboardPastePropertyValueTranslator } from '@umbraco-cms/backoffice/clipboard';

export class UmbBlockToBlockSingleClipboardPastePropertyValueTranslator
	extends UmbControllerBase
	implements UmbClipboardPastePropertyValueTranslator<UmbBlockClipboardEntryValueModel, UmbBlockSingleValueModel>
{
	/**
	 * Translates a block clipboard entry value to a block single property value.
	 * @param {UmbBlockClipboardEntryValueModel} value - The block clipboard entry value.
	 * @returns {Promise<UmbBlockSingleValueModel>} - The block single property value.
	 * @memberof UmbBlockToBlockSingleClipboardPastePropertyValueTranslator
	 */
	async translate(value: UmbBlockClipboardEntryValueModel): Promise<UmbBlockSingleValueModel> {
		if (!value) {
			throw new Error('Value is missing.');
		}

		const valueClone = structuredClone(value);

		const blockSinglePropertyValue: UmbBlockSingleValueModel = {
			contentData: valueClone.contentData,
			settingsData: valueClone.settingsData,
			expose: [],
			layout: {
				[UMB_BLOCK_SINGLE_PROPERTY_EDITOR_SCHEMA_ALIAS]: valueClone.layout ?? undefined,
			},
		};

		return blockSinglePropertyValue;
	}

	/**
	 * Checks if the clipboard entry value is compatible with the config.
	 * @param {UmbBlockSingleValueModel} propertyValue - The property value
	 * @param {*} config - The Property Editor config.
	 * @returns {Promise<boolean>} - Whether the clipboard entry value is compatible with the config.
	 * @memberof UmbBlockToBlockSingleClipboardPastePropertyValueTranslator
	 */
	async isCompatibleValue(
		propertyValue: UmbBlockSingleValueModel,
		// TODO: Replace any with the correct type.
		config: Array<{ alias: string; value: [{ contentElementTypeKey: string }] }>,
	): Promise<boolean> {
		const allowedBlockContentTypes =
			config.find((c) => c.alias === 'blocks')?.value.map((b) => b.contentElementTypeKey) ?? [];
		const blockContentTypes = propertyValue.contentData.map((c) => c.contentTypeKey);
		return blockContentTypes?.every((b) => allowedBlockContentTypes.includes(b)) ?? false;
	}
}

export { UmbBlockToBlockSingleClipboardPastePropertyValueTranslator as api };
