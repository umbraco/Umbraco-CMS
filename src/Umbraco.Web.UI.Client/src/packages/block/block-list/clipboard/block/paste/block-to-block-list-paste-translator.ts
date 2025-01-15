import { UMB_BLOCK_LIST_PROPERTY_EDITOR_SCHEMA_ALIAS } from '../../../property-editors/block-list-editor/constants.js';
import type { UmbBlockListValueModel } from '../../../types.js';
import type { UmbBlockClipboardEntryValueModel } from '@umbraco-cms/backoffice/block';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbClipboardPastePropertyValueTranslator } from '@umbraco-cms/backoffice/clipboard';

export class UmbBlockToBlockListClipboardPastePropertyValueTranslator
	extends UmbControllerBase
	implements UmbClipboardPastePropertyValueTranslator<UmbBlockClipboardEntryValueModel, UmbBlockListValueModel>
{
	/**
	 * Translates a block clipboard entry value to a block list property value.
	 * @param {UmbBlockClipboardEntryValueModel} value - The block clipboard entry value.
	 * @returns {Promise<UmbBlockListValueModel>} - The block list property value.
	 * @memberof UmbBlockToBlockListClipboardPastePropertyValueTranslator
	 */
	async translate(value: UmbBlockClipboardEntryValueModel): Promise<UmbBlockListValueModel> {
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

	/**
	 * Checks if the clipboard entry value is compatible with the config.
	 * @param {UmbBlockClipboardEntryValueModel} value - The block clipboard entry value.
	 * @param {*} config - The Property Editor config.
	 * @returns {Promise<boolean>} - Whether the clipboard entry value is compatible with the config.
	 * @memberof UmbBlockToBlockListClipboardPastePropertyValueTranslator
	 */
	async isCompatibleValue(
		value: UmbBlockClipboardEntryValueModel,
		// TODO: Replace any with the correct type.
		config: Array<{ alias: string; value: [{ contentElementTypeKey: string }] }>,
	): Promise<boolean> {
		const allowedBlockContentTypes =
			config.find((c) => c.alias === 'blocks')?.value.map((b) => b.contentElementTypeKey) ?? [];
		const blockContentTypes = value.contentData.map((c) => c.contentTypeKey);
		return blockContentTypes?.every((b) => allowedBlockContentTypes.includes(b)) ?? false;
	}
}

export { UmbBlockToBlockListClipboardPastePropertyValueTranslator as api };
