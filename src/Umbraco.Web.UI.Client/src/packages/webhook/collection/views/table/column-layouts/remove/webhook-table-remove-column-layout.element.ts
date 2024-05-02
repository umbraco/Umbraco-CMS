import type { UmbWebhookDetailModel } from '../../../../../types.js';
import { html, customElement, property, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-webhook-table-remove-column-layout')
export class UmbWebhookTableRemoveColumnLayoutElement extends UmbLitElement {
	render() {
		return html` <uui-button label="remove"></uui-button> `;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-webhook-table-remove-column-layout': UmbWebhookTableRemoveColumnLayoutElement;
	}
}
