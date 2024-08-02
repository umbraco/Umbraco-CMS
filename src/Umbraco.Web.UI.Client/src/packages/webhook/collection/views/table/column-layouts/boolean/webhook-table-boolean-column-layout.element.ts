import { html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-webhook-table-boolean-column-layout')
export class UmbWebhookTableBooleanColumnLayoutElement extends UmbLitElement {
	@property({ attribute: false })
	value = false;

	override render() {
		return html`<uui-icon name="${this.value ? 'check' : 'remove'}"></uui-icon>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-webhook-table-boolean-column-layout': UmbWebhookTableBooleanColumnLayoutElement;
	}
}
