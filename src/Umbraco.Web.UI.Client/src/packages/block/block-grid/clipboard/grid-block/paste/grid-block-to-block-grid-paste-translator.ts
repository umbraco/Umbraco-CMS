import type { UmbBlockGridPropertyEditorConfig } from '../../../property-editors/block-grid-editor/types.js';
import { UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS } from '../../../property-editors/constants.js';
import type { UmbBlockGridValueModel } from '../../../types.js';
import type { UmbGridBlockClipboardEntryValueModel } from '../../types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbClipboardPastePropertyValueTranslator } from '@umbraco-cms/backoffice/clipboard';

export class UmbGridBlockToBlockGridClipboardPastePropertyValueTranslator
	extends UmbControllerBase
	implements UmbClipboardPastePropertyValueTranslator<UmbGridBlockClipboardEntryValueModel, UmbBlockGridValueModel>
{
	/**
	 * Translates a grid block clipboard entry value to a block grid property value.
	 * @param {UmbGridBlockClipboardEntryValueModel} value - The grid block clipboard entry value.
	 * @returns {Promise<UmbBlockGridValueModel>}  {Promise<UmbBlockGridValueModel>}
	 * @memberof UmbGridBlockToBlockGridClipboardPastePropertyValueTranslator
	 */
	async translate(value: UmbGridBlockClipboardEntryValueModel): Promise<UmbBlockGridValueModel> {
		if (!value) {
			throw new Error('Value is missing.');
		}

		const valueClone = structuredClone(value);

		const blockGridPropertyValue: UmbBlockGridValueModel = {
			contentData: valueClone.contentData,
			settingsData: valueClone.settingsData,
			expose: [],
			layout: {
				[UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS]: valueClone.layout,
			},
		};

		return blockGridPropertyValue;
	}

	/**
	 * Checks if the clipboard entry value is compatible with the config.
	 * @param {UmbGridBlockClipboardEntryValueModel} value - The grid block clipboard entry value.
	 * @param {*} config - The Property Editor config.
	 * @param {(value, config) => Promise<boolean>} filter - The filter function.
	 * @returns {Promise<boolean>} {Promise<boolean>}
	 * @memberof UmbGridBlockToBlockGridClipboardPastePropertyValueTranslator
	 */
	async isCompatibleValue(
		value: UmbGridBlockClipboardEntryValueModel,
		config: UmbBlockGridPropertyEditorConfig,
		filter?: (
			value: UmbGridBlockClipboardEntryValueModel,
			config: UmbBlockGridPropertyEditorConfig,
		) => Promise<boolean>,
	): Promise<boolean> {
		const blocksConfig = config.find((c) => c.alias === 'blocks');
		const allowedBlockContentTypes = blocksConfig?.value.map((b) => b.contentElementTypeKey) ?? [];
		const blockContentTypes = value.contentData.map((c) => c.contentTypeKey);
		const allContentTypesAllowed = blockContentTypes?.every((b) => allowedBlockContentTypes.includes(b)) ?? false;
		return allContentTypesAllowed && (!filter || (await filter(value, config)));
	}
}

export { UmbGridBlockToBlockGridClipboardPastePropertyValueTranslator as api };
