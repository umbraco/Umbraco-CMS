import type { UmbDocumentItemModel } from '../repository/item/types.js';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { css, customElement, html, nothing, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

const elementName = 'umb-document-picker-search-result-item';
@customElement(elementName)
export class UmbDocumentPickerSearchResultItemElement extends UmbLitElement {
	@property({ type: Object })
	item?: UmbDocumentItemModel;

	override render() {
		if (!this.item) return nothing;

		return html`
			<uui-ref-node name=${this.item.name} id=${this.item.unique} readonly selectable>
				${this.#renderIcon()}
			</uui-ref-node>
		`;
	}

	#renderIcon() {
		if (!this.item?.documentType.icon) return nothing;
		return html`<umb-icon slot="icon" name=${this.item.documentType.icon}></umb-icon>`;
	}

	static override styles = [UmbTextStyles, css``];
}

export { UmbDocumentPickerSearchResultItemElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbDocumentPickerSearchResultItemElement;
	}
}
