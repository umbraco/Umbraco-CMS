import type { UmbBlockGridValueModel } from '../../../types.js';
import { UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS } from '../../../property-editors/constants.js';
import type { UmbGridBlockClipboardEntryValueModel } from '../../types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbClipboardCopyPropertyValueTranslator } from '@umbraco-cms/backoffice/clipboard';

export class UmbBlockGridToGridBlockClipboardCopyPropertyValueTranslator
	extends UmbControllerBase
	implements UmbClipboardCopyPropertyValueTranslator<UmbBlockGridValueModel, UmbGridBlockClipboardEntryValueModel>
{
	async translate(propertyValue: UmbBlockGridValueModel) {
		if (!propertyValue) {
			throw new Error('Property value is missing.');
		}
		const valueClone = structuredClone(propertyValue);

		const layout = valueClone.layout?.[UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS] ?? undefined;
		const contentData = valueClone.contentData;
		const settingsData = valueClone.settingsData;

		if (!layout?.length) {
			throw new Error('No layouts found.');
		}

		layout?.forEach((layoutItem) => {
			// @ts-expect-error - We are removing the $type property from the layout item
			delete layoutItem.$type;
		});

		const gridBlockValue: UmbGridBlockClipboardEntryValueModel = {
			contentData,
			layout,
			settingsData,
		};

		return gridBlockValue;
	}
}

export { UmbBlockGridToGridBlockClipboardCopyPropertyValueTranslator as api };
