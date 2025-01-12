import type { UmbBlockGridValueModel } from '../../../types.js';
import { UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS } from '../../../property-editors/constants.js';
import type { UmbGridBlockClipboardEntryValueModel } from '../../types.js';
import { forEachBlockLayoutEntryOf } from '../../../utils/index.js';
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

		return this.#constructGridBlockValue(propertyValue);
	}

	#constructGridBlockValue(propertyValue: UmbBlockGridValueModel): UmbGridBlockClipboardEntryValueModel {
		const valueClone = structuredClone(propertyValue);

		const layouts = valueClone.layout?.[UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS] ?? undefined;

		if (!layouts?.length) {
			throw new Error('No layouts found.');
		}

		layouts.forEach((layout) => {
			// Find sub Blocks and append their data:
			forEachBlockLayoutEntryOf(layout, async (entry) => {
				const content = this._manager!.getContentOf(entry.contentKey);
				if (!content) {
					throw new Error('No content found');
				}
				contentData.push(structuredClone(content));

				if (entry.settingsKey) {
					const settings = this._manager!.getSettingsOf(entry.settingsKey);
					if (settings) {
						settingsData.push(structuredClone(settings));
					}
				}
			});
		});

		const gridBlockValue: UmbGridBlockClipboardEntryValueModel = {
			contentData: valueClone.contentData,
			layout: valueClone.layout?.[UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS] ?? undefined,
			settingsData: valueClone.settingsData,
		};

		return gridBlockValue;
	}
}

export { UmbBlockGridToGridBlockClipboardCopyPropertyValueTranslator as api };
