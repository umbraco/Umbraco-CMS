import type { UmbBlockGridLayoutModel, UmbBlockGridValueModel } from '../../../types.js';
import { UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS } from '../../../constants.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbClipboardPastePropertyValueTranslator } from '@umbraco-cms/backoffice/clipboard';
import type { UmbBlockClipboardEntryValueModel, UmbBlockLayoutBaseModel } from '@umbraco-cms/backoffice/block';

export class UmbBlockToBlockGridClipboardPasteTranslator
	extends UmbControllerBase
	implements UmbClipboardPastePropertyValueTranslator<UmbBlockClipboardEntryValueModel, UmbBlockGridValueModel>
{
	async translate(value: UmbBlockClipboardEntryValueModel): Promise<UmbBlockGridValueModel> {
		if (!value) {
			throw new Error('Values is missing.');
		}

		const valueClone = structuredClone(value);

		const blockGridPropertyValue: UmbBlockGridValueModel = {
			contentData: valueClone.contentData,
			settingsData: valueClone.settingsData,
			expose: valueClone.expose,
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
}

export { UmbBlockToBlockGridClipboardPasteTranslator as api };
