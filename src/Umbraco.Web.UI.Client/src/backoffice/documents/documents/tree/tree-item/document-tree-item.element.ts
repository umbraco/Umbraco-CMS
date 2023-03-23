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

	@property({ type: Object, attribute: false })
	public item?: DocumentTreeItemResponseModel;

	render() {
		if (!this.item) return nothing;
		new UmbDocumentTreeItemContext(this, this.item);
		return html`
			<umb-tree-item-base .item=${this.item}>
				<!-- TODO: implement correct status symbol -->
				<div id="icon-container" slot="icon">
					${this.item?.icon
						? html`
								<uui-icon id="icon" slot="icon" name="${this.item.icon}"></uui-icon> <span id="status-symbol"></span>
						  `
						: nothing}
				</div>
			</umb-tree-item-base>
		`;
	}
}

export default UmbDocumentTreeItemElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-tree-item': UmbDocumentTreeItemElement;
	}
}
