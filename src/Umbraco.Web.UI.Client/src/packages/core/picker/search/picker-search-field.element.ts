import type { UmbPickerContext } from '../picker.context.js';
import { UMB_PICKER_CONTEXT } from '../picker.context.token.js';
import type { UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';
import { html, customElement, state, nothing, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

const elementName = 'umb-picker-search-field';
@customElement(elementName)
export class UmbPickerSearchFieldElement extends UmbLitElement {
	@state()
	_query: string = '';

	@state()
	_searching: boolean = false;

	@state()
	_isSearchable: boolean = false;

	#pickerContext?: UmbPickerContext;

	constructor() {
		super();

		this.consumeContext(UMB_PICKER_CONTEXT, (context) => {
			this.#pickerContext = context;
			this.observe(this.#pickerContext.search.searchable, (isSearchable) => (this._isSearchable = isSearchable));
			this.observe(this.#pickerContext.search.searching, (searching) => (this._searching = searching));
			this.observe(this.#pickerContext.search.query, (query) => (this._query = query?.query || ''));
		});
	}

	#onInput(event: UUIInputEvent) {
		const value = event.target.value as string;
		this.#pickerContext?.search.updateQuery({ query: value });
		this.#pickerContext?.search.search();
	}

	override render() {
		if (!this._isSearchable) return nothing;

		return html`
			<uui-input .value=${this._query} placeholder="Search..." @input=${this.#onInput}>
				<div slot="prepend">
					${this._searching
						? html`<uui-loader-circle id="searching-indicator"></uui-loader-circle>`
						: html`<uui-icon name="search"></uui-icon>`}
				</div>

				${this._query
					? html`
							<uui-button slot="append" type="button" @click=${() => this.#pickerContext?.search.clear()} compact>
								<uui-icon name="icon-delete"></uui-icon>
							</uui-button>
						`
					: nothing}
			</uui-input>
			<div id="divider"></div>
		`;
	}

	static override readonly styles = [
		UmbTextStyles,
		css`
			uui-input {
				width: 100%;
			}

			#divider {
				width: 100%;
				height: 1px;
				background-color: var(--uui-color-divider);
				margin-top: var(--uui-size-space-5);
				margin-bottom: var(--uui-size-space-3);
			}

			#searching-indicator {
				margin-left: 7px;
				margin-top: 4px;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbPickerSearchFieldElement;
	}
}
