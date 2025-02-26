import type { UmbBlockGridValueModel } from '../../../types.js';
import type { UmbBlockGridPropertyEditorConfig } from '../types.js';
import { UmbPasteFromClipboardPropertyAction } from '@umbraco-cms/backoffice/clipboard';

/**
 * The Block Grid Paste From Clipboard Property Action.
 * @exports
 * @class UmbBlockGridPasteFromClipboardPropertyAction
 * @augments UmbPasteFromClipboardPropertyAction
 */
export class UmbBlockGridPasteFromClipboardPropertyAction extends UmbPasteFromClipboardPropertyAction {
	/**
	 * Filters the picker based on the block grid property editor config.
	 * @param {UmbBlockGridValueModel} propertyValue The property editor value.
	 * @param {UmbBlockGridPropertyEditorConfig} config The property editor config.
	 * @override
	 * @protected
	 * @memberof UmbBlockGridPasteFromClipboardPropertyAction
	 */
	protected override async _pickerFilter(
		propertyValue: UmbBlockGridValueModel,
		config: UmbBlockGridPropertyEditorConfig,
	) {
		// The property action always paste in the root of the grid so
		// we need to check if the content types are allowed at the root
		const blocksConfig = config.find((configValue) => configValue.alias === 'blocks');

		const allowedRootContentTypeKeys =
			blocksConfig?.value
				.map((blockConfig) => {
					if (blockConfig.allowAtRoot) {
						return blockConfig.contentElementTypeKey;
					} else {
						return undefined;
					}
				})
				.filter((contentTypeKey) => contentTypeKey !== undefined) ?? [];

		// ensure all content types in the paste value are allowed in the grid root
		const rootContentKeys = propertyValue.layout['Umbraco.BlockGrid']?.map((block) => block.contentKey) ?? [];
		const rootContentTypesKeys = propertyValue.contentData
			.filter((content) => rootContentKeys.includes(content.key))
			.map((content) => content.contentTypeKey);

		const allContentTypesAllowedAtRoot = rootContentTypesKeys.every((contentKey) =>
			allowedRootContentTypeKeys.includes(contentKey),
		);

		return allContentTypesAllowedAtRoot;
	}
}
export { UmbBlockGridPasteFromClipboardPropertyAction as api };
