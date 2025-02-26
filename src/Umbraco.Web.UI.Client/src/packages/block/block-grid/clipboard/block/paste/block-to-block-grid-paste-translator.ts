import type { UmbBlockGridLayoutModel, UmbBlockGridValueModel } from '../../../types.js';
import { UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS } from '../../../constants.js';
import type { UmbBlockGridPropertyEditorConfig } from '../../../property-editors/block-grid-editor/types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbClipboardPastePropertyValueTranslator } from '@umbraco-cms/backoffice/clipboard';
import type { UmbBlockClipboardEntryValueModel, UmbBlockLayoutBaseModel } from '@umbraco-cms/backoffice/block';

export class UmbBlockToBlockGridClipboardPastePropertyValueTranslator
	extends UmbControllerBase
	implements UmbClipboardPastePropertyValueTranslator<UmbBlockClipboardEntryValueModel, UmbBlockGridValueModel>
{
	/**
	 * Translates a block clipboard entry value to a Block Grid property value.
	 * @param {UmbBlockClipboardEntryValueModel} value The block clipboard entry value.
	 * @returns {Promise<UmbBlockGridValueModel>} The translated Block Grid property value.
	 * @memberof UmbBlockToBlockGridClipboardPastePropertyValueTranslator
	 */
	async translate(value: UmbBlockClipboardEntryValueModel): Promise<UmbBlockGridValueModel> {
		if (!value) {
			throw new Error('Values is missing.');
		}

		const valueClone = structuredClone(value);

		const blockGridPropertyValue: UmbBlockGridValueModel = {
			contentData: valueClone.contentData,
			settingsData: valueClone.settingsData,
			expose: [],
			layout: {
				[UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS]: valueClone.layout?.map((baseLayout: UmbBlockLayoutBaseModel) => {
					const gridLayout: UmbBlockGridLayoutModel = {
						...baseLayout,
						columnSpan: 12,
						rowSpan: 1,
						areas: [],
					};

					return gridLayout;
				}),
			},
		};

		return blockGridPropertyValue;
	}

	/**
	 * Determines if a block clipboard entry value is compatible with the Block Grid property editor.
	 * @param {UmbBlockClipboardEntryValueModel} value The block clipboard entry value.
	 * @param {*} config The Block Grid property editor configuration.
	 * @returns {Promise<boolean>} A promise that resolves with a boolean indicating if the value is compatible.
	 * @memberof UmbBlockToBlockGridClipboardPastePropertyValueTranslator
	 */
	async isCompatibleValue(
		value: UmbBlockClipboardEntryValueModel,
		config: UmbBlockGridPropertyEditorConfig,
	): Promise<boolean> {
		const allowedBlockContentTypes =
			config.find((c) => c.alias === 'blocks')?.value.map((b) => b.contentElementTypeKey) ?? [];
		const blockContentTypes = value.contentData.map((c) => c.contentTypeKey);
		return blockContentTypes?.every((b) => allowedBlockContentTypes.includes(b)) ?? false;
	}
}

export { UmbBlockToBlockGridClipboardPastePropertyValueTranslator as api };
