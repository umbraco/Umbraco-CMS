import { UMB_BLOCK_LIST_PROPERTY_EDITOR_SCHEMA_ALIAS } from '../../property-editors/block-list-editor/constants.js';
import type { UmbBlockListLayoutModel, UmbBlockListValueModel } from '../../types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbPasteClipboardEntryTranslator } from '@umbraco-cms/backoffice/clipboard';

export class UmbCopyBlockListClipboardEntryTranslator
	extends UmbControllerBase
	implements UmbPasteClipboardEntryTranslator<UmbBlockListValueModel, any>
{
	async translate(propertyValue: UmbBlockListValueModel) {
		if (!propertyValue) {
			throw new Error('Property value is missing.');
		}

		const clone = structuredClone(propertyValue);

		const contentData = clone.contentData;
		const layout = clone.layout?.[UMB_BLOCK_LIST_PROPERTY_EDITOR_SCHEMA_ALIAS] ?? undefined;
		const settingsData = clone.settingsData;
		const expose = clone.expose;

		layout?.forEach((layoutItem: UmbBlockListLayoutModel) => {
			// @ts-expect-error - We are removing the $type property from the layout item
			delete layoutItem.$type;
		});

		return {
			contentData: contentData ?? [],
			layout: layout ?? [],
			settingsData: settingsData ?? [],
			expose: expose ?? [],
		};
	}
}

export { UmbCopyBlockListClipboardEntryTranslator as api };
