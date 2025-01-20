import type { UmbBlockGridValueModel } from '../../../types.js';
import { UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS } from '../../../property-editors/constants.js';
import type { UmbGridBlockClipboardEntryValueModel } from '../../types.js';
import { forEachBlockLayoutEntryOf } from '../../../utils/index.js';
import { UMB_BLOCK_GRID_MANAGER_CONTEXT } from '../../../context/constants.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbClipboardCopyPropertyValueTranslator } from '@umbraco-cms/backoffice/clipboard';

export class UmbBlockGridToGridBlockClipboardCopyPropertyValueTranslator
	extends UmbControllerBase
	implements UmbClipboardCopyPropertyValueTranslator<UmbBlockGridValueModel, UmbGridBlockClipboardEntryValueModel>
{
	#blockGridManager?: typeof UMB_BLOCK_GRID_MANAGER_CONTEXT.TYPE;

	async translate(propertyValue: UmbBlockGridValueModel) {
		if (!propertyValue) {
			throw new Error('Property value is missing.');
		}

		this.#blockGridManager = await this.getContext(UMB_BLOCK_GRID_MANAGER_CONTEXT);

		return this.#constructGridBlockValue(propertyValue);
	}

	#constructGridBlockValue(propertyValue: UmbBlockGridValueModel): UmbGridBlockClipboardEntryValueModel {
		const valueClone = structuredClone(propertyValue);

		const layout = valueClone.layout?.[UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS] ?? undefined;
		const contentData = valueClone.contentData;
		const settingsData = valueClone.settingsData;

		if (!layout?.length) {
			throw new Error('No layouts found.');
		}

		layout.forEach((layout) => {
			// Find sub Blocks and append their data:
			forEachBlockLayoutEntryOf(layout, async (entry) => {
				const content = this.#blockGridManager!.getContentOf(entry.contentKey);

				if (!content) {
					throw new Error('No content found');
				}

				contentData.push(structuredClone(content));

				if (entry.settingsKey) {
					const settings = this.#blockGridManager!.getSettingsOf(entry.settingsKey);
					if (settings) {
						settingsData.push(structuredClone(settings));
					}
				}
			});
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
