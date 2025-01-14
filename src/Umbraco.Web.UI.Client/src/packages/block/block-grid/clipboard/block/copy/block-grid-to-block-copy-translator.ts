import type { UmbBlockGridValueModel } from '../../../types.js';
import { UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS } from '../../../property-editors/constants.js';
import type { UmbGridBlockClipboardEntryValueModel } from '../../types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbClipboardCopyPropertyValueTranslator } from '@umbraco-cms/backoffice/clipboard';
import type { UmbBlockClipboardEntryValueModel } from '@umbraco-cms/backoffice/block';

export class UmbBlockGridToBlockClipboardCopyPropertyValueTranslator
	extends UmbControllerBase
	implements UmbClipboardCopyPropertyValueTranslator<UmbBlockGridValueModel, UmbBlockClipboardEntryValueModel>
{
	/**
	 * Translates a Block Grid property value to a Block clipboard entry value.
	 * @param {UmbBlockGridValueModel} propertyValue - The Block Grid property value.
	 * @returns {Promise<UmbBlockClipboardEntryValueModel>} - The Block clipboard entry value.
	 * @memberof UmbBlockGridToBlockClipboardCopyPropertyValueTranslator
	 */
	async translate(propertyValue: UmbBlockGridValueModel): Promise<UmbBlockClipboardEntryValueModel> {
		if (!propertyValue) {
			throw new Error('Property value is missing.');
		}

		return this.#constructBlockValue(propertyValue);
	}

	#constructGridBlockValue(propertyValue: UmbBlockGridValueModel): UmbGridBlockClipboardEntryValueModel {
		const valueClone = structuredClone(propertyValue);

		const gridBlockValue: UmbGridBlockClipboardEntryValueModel = {
			contentData: valueClone.contentData,
			layout: valueClone.layout?.[UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS] ?? undefined,
			settingsData: valueClone.settingsData,
		};

		return gridBlockValue;
	}

	#constructBlockValue(propertyValue: UmbBlockGridValueModel): UmbBlockClipboardEntryValueModel {
		const gridBlockValue = this.#constructGridBlockValue(propertyValue);

		const layout: UmbBlockClipboardEntryValueModel['layout'] = gridBlockValue.layout?.map((gridLayout) => {
			return {
				contentKey: gridLayout.contentKey,
				settingsKey: gridLayout.settingsKey,
			};
		});

		return {
			contentData: gridBlockValue.contentData,
			layout: layout,
			settingsData: gridBlockValue.settingsData,
		};
	}
}

export { UmbBlockGridToBlockClipboardCopyPropertyValueTranslator as api };
