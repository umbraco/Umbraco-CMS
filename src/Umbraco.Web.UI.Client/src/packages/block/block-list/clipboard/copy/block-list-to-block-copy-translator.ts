import { UMB_BLOCK_LIST_PROPERTY_EDITOR_SCHEMA_ALIAS } from '../../property-editors/block-list-editor/constants.js';
import type { UmbBlockListLayoutModel, UmbBlockListValueModel } from '../../types.js';
import type { UmbBlockClipboardEntryValueModel } from '@umbraco-cms/backoffice/block';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbClipboardCopyTranslator } from '@umbraco-cms/backoffice/clipboard';

export class UmbBlockListClipboardCopyTranslator
	extends UmbControllerBase
	implements UmbClipboardCopyTranslator<UmbBlockListValueModel>
{
	async translate(propertyValue: UmbBlockListValueModel): Promise<UmbBlockClipboardEntryValueModel> {
		if (!propertyValue) {
			throw new Error('Property value is missing.');
		}

		const valueClone = structuredClone(propertyValue);

		const contentData = valueClone.contentData;
		const layout = valueClone.layout?.[UMB_BLOCK_LIST_PROPERTY_EDITOR_SCHEMA_ALIAS] ?? undefined;
		const settingsData = valueClone.settingsData;
		const expose = valueClone.expose;

		layout?.forEach((layoutItem: UmbBlockListLayoutModel) => {
			// @ts-expect-error - We are removing the $type property from the layout item
			delete layoutItem.$type;
		});

		const blockValue: UmbBlockClipboardEntryValueModel = {
			contentData: contentData ?? [],
			layout: layout,
			settingsData: settingsData ?? [],
			expose: expose ?? [],
		};

		return blockValue;
	}
}

export { UmbBlockListClipboardCopyTranslator as api };
