import { css, html, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbDocumentTreeItemContext } from './document-tree-item.context.js';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { DocumentTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbTreeItemExtensionElement } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-document-tree-item')
export class UmbDocumentTreeItemElement extends UmbLitElement implements UmbTreeItemExtensionElement {
	private _item?: DocumentTreeItemResponseModel;
	@property({ type: Object, attribute: false })
	public get item() {
		return this._item;
	}
	public set item(value: DocumentTreeItemResponseModel | undefined) {
		this._item = value;
		this.#context.setTreeItem(value);
	}

	#context = new UmbDocumentTreeItemContext(this);

	render() {
		if (!this.item) return nothing;
		return html`
			<umb-tree-item-base> ${this.#renderIconWithStatusSymbol()} ${this.#renderLabel()} </umb-tree-item-base>
		`;
	}

	// TODO: implement correct status symbol
	#renderIconWithStatusSymbol() {
		return html`
			<span id="icon-container" slot="icon">
				${this.item?.icon
					? html`
							<uui-icon id="icon" slot="icon" name="${this.item.icon}"></uui-icon> <span id="status-symbol"></span>
					  `
					: nothing}
			</span>
		`;
	}

	// TODO: lower opacity if item is not published
	#renderLabel() {
		return html` <span id="label" slot="label">${this.item?.name}</span> `;
	}

	static styles = [
		UUITextStyles,
		css`
			#icon-container {
				position: relative;
			}

			#icon {
				vertical-align: middle;
			}

			#status-symbol {
				width: 8px;
				height: 8px;
				background-color: blue;
				display: block;
				position: absolute;
				bottom: 0;
				right: 0;
				border-radius: 100%;
			}
		`,
	];
}

export default UmbDocumentTreeItemElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-tree-item': UmbDocumentTreeItemElement;
	}
}
