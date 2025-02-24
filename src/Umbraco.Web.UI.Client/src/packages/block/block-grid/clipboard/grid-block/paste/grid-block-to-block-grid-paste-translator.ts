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
	 * @param {() => Promise<boolean>} filter - The filter function.
	 * @returns {Promise<boolean>} {Promise<boolean>}
	 * @memberof UmbGridBlockToBlockGridClipboardPastePropertyValueTranslator
	 */
	async isCompatibleValue(
		value: UmbGridBlockClipboardEntryValueModel,
		// TODO: Replace any with the correct type.
		config: Array<{ alias: string; value: [{ allowAtRoot: boolean; contentElementTypeKey: string }] }>,
		filter?: () => Promise<boolean>,
	): Promise<boolean> {
		const blocksConfig = config.find((c) => c.alias === 'blocks');
		const allowedBlockContentTypes = blocksConfig?.value.map((b) => b.contentElementTypeKey) ?? [];
		const blockContentTypes = value.contentData.map((c) => c.contentTypeKey);
		const allContentTypesAllowed = blockContentTypes?.every((b) => allowedBlockContentTypes.includes(b)) ?? false;

		console.log(blocksConfig);
		console.log(blockContentTypes);

		const allowedRootContentTypeKeys =
			blocksConfig?.value
				.map((blockConfig) => {
					if (blockConfig.allowAtRoot) {
						return blockConfig.contentElementTypeKey;
					} else {
						return null;
					}
				})
				.filter((contentTypeKey) => contentTypeKey !== null) ?? [];

		const allAllowedAtRoot = value.contentData.every((block) =>
			allowedRootContentTypeKeys.includes(block.contentTypeKey),
		);

		return allContentTypesAllowed && allAllowedAtRoot;
	}
}

export { UmbGridBlockToBlockGridClipboardPastePropertyValueTranslator as api };
