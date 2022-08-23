import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';
import './tree-navigator.element';
import './tree-item.element';

@customElement('umb-document-type-tree')
export class UmbDocumentTypeTree extends LitElement {
	static styles = [UUITextStyles, css``];

	@property({ type: String })
	public alias = '';

	render() {
		return html`<umb-tree-navigator .label=${this.alias}></umb-tree-navigator>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-type-tree': UmbDocumentTypeTree;
	}
}
