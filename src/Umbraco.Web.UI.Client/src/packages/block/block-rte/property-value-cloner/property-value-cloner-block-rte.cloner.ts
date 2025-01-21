import {
	UMB_BLOCK_RTE_PROPERTY_EDITOR_SCHEMA_ALIAS,
	type UmbPropertyEditorUiValueType,
} from '@umbraco-cms/backoffice/rte';
import type { UmbPropertyValueCloner } from '@umbraco-cms/backoffice/property';
import { UmbFlatLayoutBlockPropertyValueCloner } from '@umbraco-cms/backoffice/block';

export class UmbBlockRTEPropertyValueCloner implements UmbPropertyValueCloner<UmbPropertyEditorUiValueType> {
	#markup?: string;
	#markupDoc?: Document;

	async cloneValue(value: UmbPropertyEditorUiValueType) {
		if (value) {
			this.#markup = value.markup;

			const parser = new DOMParser();
			this.#markupDoc = parser.parseFromString(this.#markup, 'text/html');

			const cloner = new UmbFlatLayoutBlockPropertyValueCloner(UMB_BLOCK_RTE_PROPERTY_EDITOR_SCHEMA_ALIAS, {
				contentIdUpdatedCallback: this.#replaceContentKeyInMarkup,
			});
			const result = {} as UmbPropertyEditorUiValueType;
			result.blocks = await cloner.cloneValue(value.blocks);
			result.markup = this.#markup;
			return result;
		}
		return value;
	}

	#replaceContentKeyInMarkup = (contentKey: string, newContentKey: string) => {
		if (!this.#markupDoc) throw new Error('Markup document is not initialized');
		const elements = this.#markupDoc.querySelectorAll(
			`umb-rte-block[data-content-key='${contentKey}'], umb-rte-block-inline[data-content-key='${contentKey}']`,
		);
		elements.forEach((element) => {
			element.setAttribute('data-content-key', newContentKey);
		});
		this.#markup = this.#markupDoc.body.innerHTML ?? undefined;
	};

	destroy(): void {}
}

export { UmbBlockRTEPropertyValueCloner as api };
