import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { html, LitElement, nothing, customElement, property, css } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-webhook-table-name-column-layout')
export class UmbWebhookTableNameColumnLayoutElement extends LitElement {
	@property({ attribute: false })
	value?: { unique: string; name: string };

	override render() {
		if (!this.value) return nothing;

		return html`<a href=${'section/settings/workspace/webhook/edit/' + this.value.unique}>${this.value.name}</a>`;
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
		'umb-webhook-table-name-column-layout': UmbWebhookTableNameColumnLayoutElement;
	}
}
