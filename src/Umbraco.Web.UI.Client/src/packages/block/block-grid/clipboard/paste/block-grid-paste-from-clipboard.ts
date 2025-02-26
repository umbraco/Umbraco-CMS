import { UmbPasteFromClipboardPropertyAction } from '@umbraco-cms/backoffice/clipboard';

export class UmbBlockGridPasteFromClipboardPropertyAction extends UmbPasteFromClipboardPropertyAction {
	protected override async _pickerFilter(value: any, config: any) {
		console.log('value', value);
		console.log('config', config);
		return true;

		/*
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
		*/
	}
}
export { UmbBlockGridPasteFromClipboardPropertyAction as api };
