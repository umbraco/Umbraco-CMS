import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { html, LitElement, nothing, customElement, property, css } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-webhook-delivery-table-status-code-column-layout')
export class UmbWebhookDeliveryTableStatusCodeColumnLayoutElement extends LitElement {
	@property({ attribute: false })
	value: string = '';

	#getTagColor() {
		if (this.value.includes('(2')) {
			return 'positive';
		}
		if (this.value.includes('(4')) {
			return 'warning';
		}
		if (this.value.includes('(5')) {
			return 'danger';
		}

		return 'default';
	}

	override render() {
		if (this.value.length === 0) return nothing;
		return html`<uui-tag color=${this.#getTagColor()} look="secondary">${this.value}</uui-tag>`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				white-space: nowrap;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-webhook-delivery-table-status-code-column-layout': UmbWebhookDeliveryTableStatusCodeColumnLayoutElement;
	}
}
