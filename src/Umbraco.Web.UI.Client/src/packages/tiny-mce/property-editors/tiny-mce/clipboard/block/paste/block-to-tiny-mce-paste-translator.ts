import type { UmbBlockClipboardEntryValueModel } from '@umbraco-cms/backoffice/block';
import { UMB_BLOCK_RTE_PROPERTY_EDITOR_SCHEMA_ALIAS } from '@umbraco-cms/backoffice/block-rte';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbClipboardPastePropertyValueTranslator } from '@umbraco-cms/backoffice/clipboard';

export class UmbBlockToTinyMceClipboardPastePropertyValueTranslator
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
				expose: [],
				layout: {
					[UMB_BLOCK_RTE_PROPERTY_EDITOR_SCHEMA_ALIAS]: valueClone.layout ?? undefined,
				},
			},
			markup: this.#generateMarkup(valueClone),
		};

		return propertyValue;
	}

	#generateMarkup(value: UmbBlockClipboardEntryValueModel) {
		const layouts = value.layout ?? [];

		const markup = layouts
			.map((layout) => {
				const contentData = value.contentData.find((content) => content.key === layout.contentKey);
				const settingsData = value.settingsData.find((settings) => settings.key === layout.settingsKey);

				const contentAttr = contentData ? `data-content-key="${contentData.key}"` : '';
				const settingsAttr = settingsData ? `data-settings-key="${settingsData.key}"` : '';

				return `<umb-rte-block ${contentAttr} ${settingsAttr}></umb-rte-block>`;
			})
			.join('');

		return markup;
	}
}

export { UmbBlockToTinyMceClipboardPastePropertyValueTranslator as api };
