import { css, html, nothing } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { DocumentTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

@customElement('umb-document-tree-item')
export class UmbDocumentTreeItemElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			#label {
				display: flex;
				align-items: center;
			}

			#status-symbol {
				width: 6px;
				height: 6px;
				background-color: blue;
				display: block;
				border-radius: 3px;
				margin-right: 10px;
			}
		`,
	];

	@property({ type: Object, attribute: false })
	public item?: DocumentTreeItemResponseModel;

	render() {
		if (!this.item) return nothing;
		return html`
			<umb-tree-item-base .item=${this.item}>
				<!-- TODO: implement correct status symbol -->
				<div id="label" slot="label">
					<span id="status-symbol"></span>
					<span>${this.item?.name}</span>
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
