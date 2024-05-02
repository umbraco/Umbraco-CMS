import type { PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';
import { css, html, customElement, state, property, repeat, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import '@umbraco-cms/backoffice/culture';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

@customElement('umb-input-webhook-headers')
export class UmbInputWebhookHeadersElement extends UmbLitElement {
	@property()
	public headers: { [key: string]: string } = {};

	@state()
	private _headers: Array<{ name: string; value: string }> = [];

	@state()
	private _headerNames: string[] = ['Accept', 'Content-Type', 'User-Agent', 'Content-Length'];

	get #filterHeaderNames() {
		return this._headerNames.filter((name) => !this._headers.find((header) => header.name === name));
	}

	protected firstUpdated(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>): void {
		super.firstUpdated(_changedProperties);

		if (!this.headers) return;

		this._headers = Object.entries(this.headers).map(([name, value]) => ({ name, value }));
	}

	protected updated(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>): void {
		super.updated(_changedProperties);

		if (_changedProperties.has('_headers')) {
			this.headers = this._headers.reduce(
				(acc, header) => {
					acc[header.name as string] = header.value;
					return acc;
				},
				{} as { [key: string]: string },
			);

			this.dispatchEvent(new UmbChangeEvent());
		}
	}

	#removeHeader(index: number) {
		this._headers = this._headers.filter((_, i) => i !== index);
	}

	#onInput(event: Event, prop: keyof (typeof this._headers)[number], index: number) {
		const value = (event.target as HTMLInputElement).value;
		this._headers[index][prop] = value;
		this.requestUpdate('_headers');
	}

	#renderHeaderInput(header: { name: string; value: string }, index: number) {
		return html`
			<input
				type="text"
				placeholder="Name..."
				.value=${header.name}
				@input=${(e: InputEvent) => this.#onInput(e, 'name', index)}
				list="nameList" />
			<input
				type="text"
				placeholder="Value..."
				.value=${header.value}
				@input=${(e: InputEvent) => this.#onInput(e, 'value', index)}
				list="valueList" />
			<button @click=${() => this.#removeHeader(index)}>Remove</button>
		`;
	}

	#renderHeaders() {
		if (!this._headers.length) return nothing;

		return html`
			<span>Name</span>
			<span>Value</span>
			<span></span>
			${repeat(
				this._headers,
				(_, index) => index,
				(header, index) => this.#renderHeaderInput(header, index),
			)}
			<datalist id="nameList">
				${repeat(
					this.#filterHeaderNames,
					(name) => name,
					(name) => html`<option value=${name}></option>`,
				)}
			</datalist>
			<datalist id="valueList">
				<option value="application/json"></option>
			</datalist>
		`;
	}

	#addHeader() {
		this._headers = [...this._headers, { name: '', value: '' }];
	}

	render() {
		return html`
			${this.#renderHeaders()}

			<uui-button id="add" look="placeholder" @click=${this.#addHeader}>Add</uui-button>
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

			input {
				width: 100%;
				border: none;
				font: inherit;
			}

			/* Remove arrow in inputs linked to a datalist */
			input[type='text']::-webkit-calendar-picker-indicator {
				display: none !important;
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
