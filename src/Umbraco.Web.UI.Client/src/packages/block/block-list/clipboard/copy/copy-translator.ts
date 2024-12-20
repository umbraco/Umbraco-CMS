import { UMB_BLOCK_LIST_PROPERTY_EDITOR_SCHEMA_ALIAS } from '../../property-editors/block-list-editor/constants.js';
import type { UmbBlockListLayoutModel, UmbBlockListValueModel } from '../../types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbClipboardCopyTranslator } from '@umbraco-cms/backoffice/clipboard';

export class UmbBlockListClipboardCopyTranslator
	extends UmbControllerBase
	implements UmbClipboardCopyTranslator<UmbBlockListValueModel>
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
			// TODO: this is just for testing purposes, remove this before merging
			{
				type: 'text',
				value: JSON.stringify(blockValue),
			},
		];
	}
}

export { UmbBlockListClipboardCopyTranslator as api };
