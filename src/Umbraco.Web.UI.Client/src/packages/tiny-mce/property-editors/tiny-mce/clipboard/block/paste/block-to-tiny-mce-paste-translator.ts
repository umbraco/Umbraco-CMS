import type { UmbBlockClipboardEntryValueModel } from '@umbraco-cms/backoffice/block';
import { UMB_BLOCK_RTE_PROPERTY_EDITOR_SCHEMA_ALIAS } from '@umbraco-cms/backoffice/block-rte';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbClipboardPastePropertyValueTranslator } from '@umbraco-cms/backoffice/clipboard';

export class UmbBlockToBlockListClipboardPastePropertyValueTranslator
	extends UmbControllerBase
	implements UmbClipboardPastePropertyValueTranslator<UmbBlockClipboardEntryValueModel, any>
{
	async translate(value: UmbBlockClipboardEntryValueModel) {
		if (!value) {
			throw new Error('Value is missing.');
		}

		const valueClone = structuredClone(value);

		// Todo: add type for property value
		const propertyValue: any = {
			blocks: {
				contentData: valueClone.contentData,
				settingsData: valueClone.settingsData,
				expose: valueClone.expose,
				layout: {
					[UMB_BLOCK_RTE_PROPERTY_EDITOR_SCHEMA_ALIAS]: valueClone.layout ?? undefined,
				},
			},
			markup: this.#getMarkup(valueClone),
		};

		return propertyValue;
	}

	#getMarkup(value: UmbBlockClipboardEntryValueModel) {
		const hasContentData = value.contentData && value.contentData.length > 0;
		const contentAttr = hasContentData ? `data-content-key="${value.contentData[0].key}"` : '';
		const hasSettingsData = value.settingsData && value.settingsData.length > 0;
		const settingsAttr = hasSettingsData ? `data-settings-key="${value.settingsData[0].key}"` : '';
		return `<umb-rte-block ${contentAttr} ${settingsAttr}></umb-rte-block>`;
	}
}

export { UmbBlockToBlockListClipboardPastePropertyValueTranslator as api };
