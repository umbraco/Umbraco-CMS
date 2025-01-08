import type { UmbBlockGridValueModel } from '../../../types.js';
import { UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS } from '../../../property-editors/constants.js';
import type { UmbGridBlockClipboardEntryValueModel } from '../../types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbClipboardCopyTranslator } from '@umbraco-cms/backoffice/clipboard';
import type { UmbBlockClipboardEntryValueModel } from '@umbraco-cms/backoffice/block';

export class UmbBlockGridToBlockClipboardCopyTranslator
	extends UmbControllerBase
	implements UmbClipboardCopyTranslator<UmbBlockGridValueModel, UmbBlockClipboardEntryValueModel>
{
	async translate(propertyValue: UmbBlockGridValueModel) {
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
			expose: valueClone.expose,
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
			expose: gridBlockValue.expose,
		};
	}
}

export { UmbBlockGridToBlockClipboardCopyTranslator as api };
