import { html, nothing, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-webhook-table-boolean-column-layout')
export class UmbWebhookTableBooleanColumnLayoutElement extends UmbLitElement {
	@property({ attribute: false })
	value = false;

	render() {
		return this.value ? html`<uui-icon name="icon-check"></uui-icon>` : nothing;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-webhook-table-boolean-column-layout': UmbWebhookTableBooleanColumnLayoutElement;
	}
}
