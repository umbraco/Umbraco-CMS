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
	private _headerNames: string[] = ['Accept', 'Content-Length', 'Content-Type', 'User-Agent'];

	get #filterHeaderNames() {
		return this._headerNames.filter((name) => !this._headers.find((header) => header.name === name));
	}

	protected override firstUpdated(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>): void {
		super.firstUpdated(_changedProperties);

		if (!this.headers) return;

		this._headers = Object.entries(this.headers).map(([name, value]) => ({ name, value }));
	}

	protected override updated(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>): void {
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

	#addHeader() {
		this._headers = [...this._headers, { name: '', value: '' }];

		requestAnimationFrame(() => {
			// Focus newly added input
			const inputs = this.shadowRoot?.querySelectorAll('input[type="text"]');
			const lastInput = inputs?.[inputs.length - 2] as HTMLInputElement | undefined;
			lastInput?.focus();
		});
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
		//TODO: Use uui-input when it supports datalist
		return html`
			<input
				type="text"
				.value=${header.name}
				@input=${(e: InputEvent) => this.#onInput(e, 'name', index)}
				list="nameList" />
			<input
				type="text"
				.value=${header.value}
				@input=${(e: InputEvent) => this.#onInput(e, 'value', index)}
				list="valueList" />
			<uui-button @click=${() => this.#removeHeader(index)} label=${this.localize.term('general_remove')}></uui-button>
		`;
	}

	#renderGrid() {
		if (!this._headers.length) return nothing;

		return html`
			<div id="grid">${this.#renderHeaders()}</div>

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

	#renderHeaders() {
		if (!this._headers.length) return nothing;

		return html`
			<span class="grid-top"><umb-localize key="general_name">Name</umb-localize></span>
			<span class="grid-top"><umb-localize key="general_value">Value</umb-localize></span>
			<span class="grid-top"></span>
			${repeat(
				this._headers,
				(_, index) => index,
				(header, index) => this.#renderHeaderInput(header, index),
			)}
		`;
	}

	override render() {
		return html`
			${this.#renderGrid()}
			<uui-button id="add" look="placeholder" @click=${this.#addHeader}>Add</uui-button>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			#grid {
				display: grid;
				grid-template-columns: 1fr 1fr auto;
				border: 1px solid var(--uui-color-border);
				margin-bottom: var(--uui-size-space-3);
				border-radius: var(--uui-border-radius);
			}

			.grid-top {
				background-color: var(--uui-color-surface-alt);
			}

			#grid > * {
				border-right: 1px solid var(--uui-color-border);
				border-bottom: 1px solid var(--uui-color-border);
			}

			#grid > *:nth-child(3) {
				border-top-right-radius: var(--uui-border-radius);
			}

			#grid > *:nth-child(1) {
				border-top-left-radius: var(--uui-border-radius);
			}

			/* Remove borders from last column */
			#grid > *:nth-child(3n) {
				border-right: none;
			}

			/* Remove borders from last row */
			#grid > *:nth-child(3n + 1):nth-last-child(-n + 3),
			#grid > *:nth-child(3n + 1):nth-last-child(-n + 3) ~ * {
				border-bottom: none;
			}

			#grid > *:not(uui-button) {
				padding: var(--uui-size-2) var(--uui-size-space-4);
			}

			uui-button {
				width: 100%;
				box-sizing: border-box;
			}

			input {
				width: 100%;
				border: none;
				font: inherit;
				color: inherit;
				display: flex;
				box-sizing: border-box;
				background-color: transparent;
			}

			input:focus,
			uui-button:focus {
				z-index: 1;
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
