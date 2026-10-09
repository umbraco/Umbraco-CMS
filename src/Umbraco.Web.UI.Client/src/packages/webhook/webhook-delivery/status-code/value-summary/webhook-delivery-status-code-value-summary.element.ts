import type { UmbWebhookDeliveryStatusCodeValue } from '../value-type/constants.js';
import { css, customElement, html, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbValueSummaryElementBase } from '@umbraco-cms/backoffice/value-summary';

@customElement('umb-webhook-delivery-status-code-value-summary')
export class UmbWebhookDeliveryStatusCodeValueSummaryElement extends UmbValueSummaryElementBase<UmbWebhookDeliveryStatusCodeValue | null> {
	#getTagColor() {
		const code = this._value?.code;
		if (code == null) return 'default';
		if (code >= 200 && code < 300) return 'positive';
		if (code >= 400 && code < 500) return 'warning';
		if (code >= 500) return 'danger';
		return 'default';
	}

	override render() {
		if (!this._value?.label) return nothing;
		return html`<uui-tag color=${this.#getTagColor()} look="secondary">${this._value.label}</uui-tag>`;
	}

	static override styles = css`
		:host {
			white-space: nowrap;
		}
	`;
}

export { UmbWebhookDeliveryStatusCodeValueSummaryElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-webhook-delivery-status-code-value-summary': UmbWebhookDeliveryStatusCodeValueSummaryElement;
	}
}
