import { css, customElement, html, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbValueSummaryElementBase } from '@umbraco-cms/backoffice/value-summary';

@customElement('umb-webhook-delivery-status-code-value-summary')
export class UmbWebhookDeliveryStatusCodeValueSummaryElement extends UmbValueSummaryElementBase<string | null> {
	#getTagColor() {
		if (this._value?.includes('(2')) return 'positive';
		if (this._value?.includes('(4')) return 'warning';
		if (this._value?.includes('(5')) return 'danger';
		return 'default';
	}

	override render() {
		if (!this._value) return nothing;
		return html`<uui-tag color=${this.#getTagColor()} look="secondary">${this._value}</uui-tag>`;
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
