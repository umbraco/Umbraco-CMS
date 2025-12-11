import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UMB_BLOCK_RTE_PROPERTY_EDITOR_SCHEMA_ALIAS } from '@umbraco-cms/backoffice/block-rte';
import type { UmbBlockClipboardEntryValueModel } from '@umbraco-cms/backoffice/block';
import type { UmbClipboardCopyPropertyValueTranslator } from '@umbraco-cms/backoffice/clipboard';
import type { UmbBlockRteLayoutModel, UmbBlockRteValueModel } from '@umbraco-cms/backoffice/block-rte';

export class UmbTiptapBlockRteToBlockClipboardCopyPropertyValueTranslator
	extends UmbControllerBase
	implements UmbClipboardCopyPropertyValueTranslator<UmbBlockRteValueModel>
{
	async translate(propertyValue: UmbBlockRteValueModel): Promise<UmbBlockClipboardEntryValueModel> {
		if (!propertyValue) {
			throw new Error('Property value is missing.');
		}

		const valueClone = structuredClone(propertyValue);

		const contentData = valueClone.contentData;
		const layout = valueClone.layout?.[UMB_BLOCK_RTE_PROPERTY_EDITOR_SCHEMA_ALIAS] ?? undefined;
		const settingsData = valueClone.settingsData;

		layout?.forEach((layoutItem: UmbBlockRteLayoutModel) => {
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

export { UmbTiptapBlockRteToBlockClipboardCopyPropertyValueTranslator as api };
