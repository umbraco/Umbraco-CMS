import type { UmbGridBlockClipboardEntryValueModel } from '../../../types.js';
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
	 * @param {UmbBlockGridPropertyEditorUiValue} value The property editor value.
	 * @param {UmbBlockGridPropertyEditorConfig} config The property editor config.
	 * @override
	 * @protected
	 * @memberof UmbBlockGridPasteFromClipboardPropertyAction
	 */
	protected override async _pickerFilter(
		value: UmbGridBlockClipboardEntryValueModel,
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
						return null;
					}
				})
				.filter((contentTypeKey) => contentTypeKey !== null) ?? [];

		// ensure all content types in the paste value are allowed in the grid root
		return value.contentData.every((block) => allowedRootContentTypeKeys.includes(block.contentTypeKey));
	}
}
export { UmbBlockGridPasteFromClipboardPropertyAction as api };
