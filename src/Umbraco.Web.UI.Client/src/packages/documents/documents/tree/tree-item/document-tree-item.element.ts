import type { UmbDocumentTreeItemModel } from '../types.js';
import { css, html, nothing, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbTreeItemElementBase } from '@umbraco-cms/backoffice/tree';

@customElement('umb-document-tree-item')
export class UmbDocumentTreeItemElement extends UmbTreeItemElementBase<UmbDocumentTreeItemModel> {
	// TODO: implement correct status symbol
	renderIconContainer() {
		return html`
			<span id="icon-container" slot="icon">
				${this.item?.documentType.icon
					? html`
							<umb-icon id="icon" slot="icon" name="${this.item.documentType.icon}"></umb-icon>
							<span id="status-symbol"></span>
					  `
					: nothing}
			</span>
		`;
	}

	// TODO: lower opacity if item is not published
	// TODO: get correct variant name
	renderLabel() {
		return html`<span id="label" slot="label">${this.item?.variants[0].name}</span> `;
	}

	static styles = [
		UmbTextStyles,
		css`
			#icon-container {
				position: relative;
			}

			#icon {
				vertical-align: middle;
			}

			#status-symbol {
				width: 5px;
				height: 5px;
				border: 1px solid white;
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
