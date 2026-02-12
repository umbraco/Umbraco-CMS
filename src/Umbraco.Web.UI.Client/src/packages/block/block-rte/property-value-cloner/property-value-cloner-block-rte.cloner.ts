import {
	UMB_BLOCK_RTE_PROPERTY_EDITOR_SCHEMA_ALIAS,
	type UmbPropertyEditorRteValueType,
} from '@umbraco-cms/backoffice/rte';
import type { UmbPropertyValueCloner } from '@umbraco-cms/backoffice/property';
import { UmbFlatLayoutBlockPropertyValueCloner } from '@umbraco-cms/backoffice/block';

export class UmbBlockRTEPropertyValueCloner implements UmbPropertyValueCloner<UmbPropertyEditorRteValueType> {
	#markup?: string;
	#markupDoc?: Document;

	async cloneValue(value: UmbPropertyEditorRteValueType) {
		if (!value) return value;

		const result = {} as UmbPropertyEditorRteValueType;

		if (value.blocks) {
			if (value.markup) {
				// Full path - parse DOM and update content keys in markup
				this.#markup = value.markup;
				this.#markupDoc = new DOMParser().parseFromString(this.#markup, 'text/html');

				const cloner = new UmbFlatLayoutBlockPropertyValueCloner(UMB_BLOCK_RTE_PROPERTY_EDITOR_SCHEMA_ALIAS, {
					contentIdUpdatedCallback: this.#replaceContentKeyInMarkup,
				});
				result.blocks = await cloner.cloneValue(value.blocks);
				result.markup = this.#markup;
			} else {
				// Fast path - no markup to update, just clone blocks
				const cloner = new UmbFlatLayoutBlockPropertyValueCloner(UMB_BLOCK_RTE_PROPERTY_EDITOR_SCHEMA_ALIAS);
				result.blocks = await cloner.cloneValue(value.blocks);
				result.markup = '';
			}
		} else {
			result.markup = value.markup ?? '';
		}

		return result;
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
