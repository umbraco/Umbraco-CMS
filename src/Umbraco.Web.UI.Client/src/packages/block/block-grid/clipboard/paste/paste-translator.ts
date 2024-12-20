import type { UmbBlockGridLayoutModel, UmbBlockGridValueModel } from '../../types.js';
import { UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS } from '../../constants.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbClipboardPasteTranslator } from '@umbraco-cms/backoffice/clipboard';
import type { UmbBlockLayoutBaseModel } from '@umbraco-cms/backoffice/block';

export class UmbBlockGridClipboardPasteTranslator
	extends UmbControllerBase
	implements UmbClipboardPasteTranslator<any, UmbBlockGridValueModel>
{
	async translate(clipboardEntryValue: any) {
		if (!clipboardEntryValue) {
			throw new Error('Clipboard entry value is missing.');
		}

		const clone = structuredClone(clipboardEntryValue);

		const propertyValue: UmbBlockGridValueModel = {
			contentData: clone.contentData,
			settingsData: clone.settingsData,
			expose: clone.expose,
			layout: {
				[UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS]: clone.layout.map((baseLayout: UmbBlockLayoutBaseModel) => {
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

		return propertyValue;
	}
}

export { UmbBlockGridClipboardPasteTranslator as api };
