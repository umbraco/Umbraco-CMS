import type { UmbBlockGridValueModel } from '../../types.js';
import { UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS } from '../../property-editors/constants.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbClipboardCopyTranslator } from '@umbraco-cms/backoffice/clipboard';

export class UmbBlockGridClipboardCopyTranslator
	extends UmbControllerBase
	implements UmbClipboardCopyTranslator<UmbBlockGridValueModel>
{
	async translate(propertyValue: UmbBlockGridValueModel) {
		if (!propertyValue) {
			throw new Error('Property value is missing.');
		}

		return [
			{
				type: 'gridBlock',
				value: this.#constructGridBlockValue(propertyValue),
			},
			{
				type: 'block',
				value: this.#constructBlockValue(propertyValue),
			},
		];
	}

	#constructGridBlockValue(propertyValue: UmbBlockGridValueModel) {
		const clone = structuredClone(propertyValue);

		const contentData = clone.contentData;
		const layout = clone.layout?.[UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS] ?? undefined;
		const settingsData = clone.settingsData;
		const expose = clone.expose;

		const gridBlockValue = {
			contentData: contentData ?? [],
			layout: layout ?? [],
			settingsData: settingsData ?? [],
			expose: expose ?? [],
		};

		return gridBlockValue;
	}

	#constructBlockValue(propertyValue: UmbBlockGridValueModel): UmbBlockGridValueModel {
		const gridBlockValue = this.#constructGridBlockValue(propertyValue);

		const layout = gridBlockValue.layout.map((gridLayout) => {
			delete gridLayout.areas;
			delete gridLayout.columnSpan;
			delete gridLayout.rowSpan;
			return gridLayout.$type;
		});
		debugger;

		return {
			contentData: gridBlockValue.contentData,
			layout: layout,
			settingsData: gridBlockValue.settingsData,
			expose: gridBlockValue.expose,
		};
	}
}

export { UmbBlockGridClipboardCopyTranslator as api };
