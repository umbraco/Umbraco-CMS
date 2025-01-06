import type { UmbBlockGridLayoutModel, UmbBlockGridValueModel } from '../../types.js';
import { UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS } from '../../constants.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbClipboardEntryValueModel, UmbClipboardPasteTranslator } from '@umbraco-cms/backoffice/clipboard';
import type { UmbBlockLayoutBaseModel } from '@umbraco-cms/backoffice/block';

export class UmbBlockToBlockGridClipboardPasteTranslator
	extends UmbControllerBase
	implements UmbClipboardPasteTranslator<UmbBlockGridValueModel>
{
	async translate(model: UmbClipboardEntryValueModel) {
		if (!model) {
			throw new Error('Values is missing.');
		}

		if (model.type !== 'block') {
			throw new Error('Invalid value type.');
		}

		const valueClone = structuredClone(model.value);

		const blockGridPropertyValue: UmbBlockGridValueModel = {
			contentData: valueClone.contentData,
			settingsData: valueClone.settingsData,
			expose: valueClone.expose,
			layout: {
				[UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS]: valueClone.layout.map((baseLayout: UmbBlockLayoutBaseModel) => {
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
