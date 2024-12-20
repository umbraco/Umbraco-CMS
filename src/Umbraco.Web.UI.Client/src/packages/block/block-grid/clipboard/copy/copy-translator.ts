import type { UmbBlockGridLayoutModel, UmbBlockGridValueModel } from '../../types.js';
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

		const clone = structuredClone(propertyValue);

		const contentData = clone.contentData;
		const layout = clone.layout?.[UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS] ?? undefined;
		const settingsData = clone.settingsData;
		const expose = clone.expose;

		layout?.forEach((layoutItem: UmbBlockGridLayoutModel) => {
			// @ts-expect-error - We are removing the $type property from the layout item
			delete layoutItem.$type;
		});

		const blockValue = {
			contentData: contentData ?? [],
			layout: layout ?? [],
			settingsData: settingsData ?? [],
			expose: expose ?? [],
		};

		return [
			{
				type: 'block',
				value: blockValue,
			},
			{
				type: 'gridBlock',
				value: 'This is a grid block value',
			},
		];
	}
}

export { UmbBlockGridClipboardCopyTranslator as api };
