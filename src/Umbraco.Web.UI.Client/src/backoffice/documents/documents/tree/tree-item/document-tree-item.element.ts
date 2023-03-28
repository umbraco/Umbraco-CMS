import { css, html, nothing } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';
import { UmbDocumentTreeItemContext } from './document-tree-item.context';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { DocumentTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

@customElement('umb-document-tree-item')
export class UmbDocumentTreeItemElement extends UmbLitElement {
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
}

export default UmbDocumentTreeItemElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-tree-item': UmbDocumentTreeItemElement;
	}
}
