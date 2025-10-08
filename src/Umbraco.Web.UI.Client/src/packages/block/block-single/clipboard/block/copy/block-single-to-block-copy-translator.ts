import { UMB_BLOCK_SINGLE_PROPERTY_EDITOR_SCHEMA_ALIAS } from '../../../property-editors/block-single-editor/constants.js';
import type { UmbBlockSingleLayoutModel, UmbBlockSingleValueModel } from '../../../types.js';
import type { UmbBlockClipboardEntryValueModel } from '@umbraco-cms/backoffice/block';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbClipboardCopyPropertyValueTranslator } from '@umbraco-cms/backoffice/clipboard';

export class UmbBlockSingleToBlockClipboardCopyPropertyValueTranslator
	extends UmbControllerBase
	implements UmbClipboardCopyPropertyValueTranslator<UmbBlockSingleValueModel>
{
	async translate(propertyValue: UmbBlockSingleValueModel): Promise<UmbBlockClipboardEntryValueModel> {
		if (!propertyValue) {
			throw new Error('Property value is missing.');
		}

		const valueClone = structuredClone(propertyValue);

		const contentData = valueClone.contentData;
		const layout = valueClone.layout?.[UMB_BLOCK_SINGLE_PROPERTY_EDITOR_SCHEMA_ALIAS] ?? undefined;
		const settingsData = valueClone.settingsData;

		layout?.forEach((layoutItem: UmbBlockSingleLayoutModel) => {
			// @ts-expect-error - We are removing the $type property from the layout item
			delete layoutItem.$type;
		});

		const blockValue: UmbBlockClipboardEntryValueModel = {
			contentData: contentData ?? [],
			layout: layout,
			settingsData: settingsData ?? [],
		};

		return blockValue;
	}
}

export { UmbBlockSingleToBlockClipboardCopyPropertyValueTranslator as api };
