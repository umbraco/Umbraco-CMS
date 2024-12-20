import type { UmbBlockGridLayoutModel, UmbBlockGridValueModel } from '../../types.js';
import { UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS } from '../../constants.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbClipboardEntryValuesType, UmbClipboardPasteTranslator } from '@umbraco-cms/backoffice/clipboard';
import type { UmbBlockLayoutBaseModel } from '@umbraco-cms/backoffice/block';

export class UmbBlockGridClipboardPasteTranslator
	extends UmbControllerBase
	implements UmbClipboardPasteTranslator<UmbBlockGridValueModel>
{
	async translate(values: UmbClipboardEntryValuesType) {
		if (!values.length) {
			throw new Error('Values are missing.');
		}

		const blockTypeValue = values.find((x) => x.type === 'block');
		const clone = structuredClone(blockTypeValue?.value);

		const blockGridPropertyValue: UmbBlockGridValueModel = {
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

		return [blockGridPropertyValue];
	}
}

export { UmbBlockGridClipboardPasteTranslator as api };
