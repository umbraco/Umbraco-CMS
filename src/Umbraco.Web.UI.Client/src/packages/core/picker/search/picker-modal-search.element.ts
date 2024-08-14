import type { UmbPickerModalContext } from '../picker-modal.context.js';
import { UMB_PICKER_MODAL_CONTEXT } from '../picker-modal.context.token.js';
import type { UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';
import { html, customElement, state, nothing, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

const elementName = 'umb-picker-modal-search';
@customElement(elementName)
export class UmbPickerModalSearchElement extends UmbLitElement {
	@state()
	_query: string = '';

	@state()
	_searching: boolean = false;

	#pickerModalContext?: UmbPickerModalContext;

	constructor() {
		super();

		this.consumeContext(UMB_PICKER_MODAL_CONTEXT, (context) => {
			this.#pickerModalContext = context;

			this.observe(this.#pickerModalContext.search.searching, (searching) => {
				this._searching = searching;
			});
		});
	}

	#onInput(event: UUIInputEvent) {
		const value = event.target.value as string;
		this.#pickerModalContext?.search.setQuery({ query: value });
		this.#pickerModalContext?.search.search();
	}

	override render() {
		return html`
			<uui-input .value=${this._query} placeholder="Search..." @input=${this.#onInput}>
				<div slot="prepend">
					${this._searching
						? html`<uui-loader-circle id="searching-indicator"></uui-loader-circle>`
						: html`<uui-icon name="search"></uui-icon>`}
				</div>

				${this._query
					? html`
							<uui-button slot="append" type="button" @click=${() => this.#pickerModalContext?.search.clear()} compact>
								<uui-icon name="icon-delete"></uui-icon>
							</uui-button>
						`
					: nothing}
			</uui-input>
			<div id="divider"></div>
		`;
	}

	static override styles = [
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
		[elementName]: UmbPickerModalSearchElement;
	}
}
