import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { html, LitElement, nothing, customElement, property, css } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-webhook-delivery-table-status-code-column-layout')
export class UmbWebhookDeliveryTableStatusCodeColumnLayoutElement extends LitElement {
	@property({ attribute: false })
	value: string = "";

	override render() {
		if (this.value.length === 0) return nothing;

		const className = this.value.includes("(2") || this.value.includes("(3")
			? "success"
			: this.value.includes("(4")
				? "warning"
				: "error";

		return html`<span class="${className}">${this.value}</span>`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				white-space: nowrap;
			}

			.success { color: var(--uui-palette-jungle-green); }
			.warning { color: var(--uui-palette-chamoisee); }
			.error { color: var(--uui-palette-maroon-flush); }
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-webhook-delivery-table-status-code-column-layout': UmbWebhookDeliveryTableStatusCodeColumnLayoutElement;
	}
}
