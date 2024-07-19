import type { UmbEditableDocumentCollectionItemModel } from '../../../types.js';
import { css, customElement, html, nothing, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbTableColumn, UmbTableColumnLayoutElement, UmbTableItem } from '@umbraco-cms/backoffice/components';
import type { UUIButtonElement } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-document-table-column-name')
export class UmbDocumentTableColumnNameElement extends UmbLitElement implements UmbTableColumnLayoutElement {
	column!: UmbTableColumn;
	item!: UmbTableItem;

	@property({ attribute: false })
	value!: UmbEditableDocumentCollectionItemModel;

	#onClick(event: Event & { target: UUIButtonElement }) {
		event.preventDefault();
		event.stopPropagation();
		window.history.pushState(null, '', event.target.href);
	}

	override render() {
		if (!this.value) return nothing;
		return html`
			<uui-button
				compact
				href=${this.value.editPath}
				label=${this.value.item.name}
				@click=${this.#onClick}></uui-button>
		`;
	}

	static override styles = [
		css`
			uui-button {
				text-align: left;
			}
		`,
	];
}

export default UmbDocumentTableColumnNameElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-table-column-name': UmbDocumentTableColumnNameElement;
	}
}
