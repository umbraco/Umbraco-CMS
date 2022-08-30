import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement } from 'lit/decorators.js';

@customElement('umb-entity-action-document-type-create')
export class UmbEntityActionDocumentTypeCreateElement extends LitElement {
	static styles = [UUITextStyles, css``];

	render() {
		return html`<div>Create Document Type Extension</div>`;
	}
}

export default UmbEntityActionDocumentTypeCreateElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-entity-action-document-type-create': UmbEntityActionDocumentTypeCreateElement;
	}
}
