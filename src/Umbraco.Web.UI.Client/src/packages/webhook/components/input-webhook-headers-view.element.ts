import type { PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';
import { css, html, customElement, state, property, repeat, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import '@umbraco-cms/backoffice/culture';

@customElement('umb-input-webhook-headers')
export class UmbInputWebhookHeadersElement extends UmbLitElement implements UmbWorkspaceViewElement {
	@state()
	headers: Array<{ name: string; value: string }> = [];

	protected updated(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>): void {
		super.updated(_changedProperties);

		if (_changedProperties.has('headers')) {
			console.log('Headers changed', this.headers);
		}
	}

	#removeHeader(index: number) {
		this.headers = this.headers.filter((_, i) => i !== index);
	}

	#onInput(event: Event, prop: keyof (typeof this.headers)[number], index: number) {
		const value = (event.target as HTMLInputElement).value;

		this.headers[index][prop] = value;
		this.requestUpdate('headers');
	}

	#renderHeaderInput(header: { name: string; value: string }, index: number) {
		return html`
			<input type="text" .value=${header.name} @input=${(e: InputEvent) => this.#onInput(e, 'name', index)} />
			<input type="text" .value=${header.value} @input=${(e: InputEvent) => this.#onInput(e, 'value', index)} />
			<button @click=${() => this.#removeHeader(index)}>Remove</button>
		`;
	}

	#renderHeaders() {
		if (!this.headers.length) return nothing;

		return html`
			<span>Name</span>
			<span>Value</span>
			<span></span>
			${repeat(
				this.headers,
				(_, index) => index,
				(header, index) => this.#renderHeaderInput(header, index),
			)}
		`;
	}

	#addHeader() {
		this.headers = [...this.headers, { name: '', value: '' }];
	}

	render() {
		return html`
			${this.#renderHeaders()}

			<uui-button id="add" look="primary" @click=${this.#addHeader}>Add</uui-button>
		`;
	}

	static styles = [
		UmbTextStyles,
		css`
			:host {
				display: grid;
				grid-template-columns: 1fr 1fr auto;
				gap: var(--uui-size-space-2) var(--uui-size-space-2);
			}

			#add {
				grid-column: -1 / 1;
			}
		`,
	];
}

export default UmbInputWebhookHeadersElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-webhook-headers': UmbInputWebhookHeadersElement;
	}
}
