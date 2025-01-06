import type { UmbBlockGridValueModel } from '../../types.js';
import { UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS } from '../../property-editors/constants.js';
import type { UmbGridBlockClipboardEntryValueModel } from '../types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbClipboardCopyTranslator } from '@umbraco-cms/backoffice/clipboard';

export class UmbBlockGridToGridBlockClipboardCopyTranslator
	extends UmbControllerBase
	implements UmbClipboardCopyTranslator<UmbBlockGridValueModel, UmbGridBlockClipboardEntryValueModel>
{
	async translate(propertyValue: UmbBlockGridValueModel) {
		if (!propertyValue) {
			throw new Error('Property value is missing.');
		}

		return this.#constructGridBlockValue(propertyValue);
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
}

export { UmbBlockGridToGridBlockClipboardCopyTranslator as api };
