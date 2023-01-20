import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { repeat } from 'lit/directives/repeat.js';
import { UUIInputElement, UUIInputEvent } from '@umbraco-ui/uui-input';
import { UmbPropertyValueChangeEvent } from '../..';

export type MultipleTextStringConfigData = Array<{
	alias: 'minNumber' | 'maxNumber';
	value: number;
}>;

export type MultipleTextStringValue = Array<MultipleTextStringValueItem>;

export interface MultipleTextStringValueItem {
	value: string;
}

/**
 * @element umb-property-editor-ui-multiple-text-string
 */
@customElement('umb-property-editor-ui-multiple-text-string')
export class UmbPropertyEditorUIMultipleTextStringElement extends LitElement {
	static styles = [
		UUITextStyles,
		css`
			.item {
				display: flex;
				align-items: center;
				margin-bottom: var(--uui-size-space-3);
				gap: var(--uui-size-space-3);
			}

			.item #text-field {
				flex: 1;
			}

			#action {
				display: block;
			}
		`,
	];

	@state()
	private _value: MultipleTextStringValue = [];

	@property({ type: Array })
	public get value(): MultipleTextStringValue {
		return this._value;
	}
	public set value(value: MultipleTextStringValue) {
		this._value = value || [];
	}

	@property({ type: Array, attribute: false })
	public set config(config: MultipleTextStringConfigData) {
		this._limitMin = config.find((x) => x.alias === 'minNumber')?.value;
		this._limitMax = config.find((x) => x.alias === 'maxNumber')?.value;
	}

	@state()
	private _limitMin?: number;

	@state()
	private _limitMax?: number;

	private _onAdd() {
		this._value = [...this._value, { value: '' }];
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
		this._focusNewItem();
	}

	private _onInput(event: UUIInputEvent) {
		const target = event.target as UUIInputElement;
		const value = target.value as string;
		const valueIndex = Number(target.dataset.valueIndex);
		this._value = this._value.map((item, index) => (index === valueIndex ? { value } : item));
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	private async _focusNewItem() {
		await this.updateComplete;
		const inputs = this.shadowRoot?.querySelectorAll('uui-input') as NodeListOf<UUIInputElement>;
		const lastInput = inputs[inputs.length - 1];
		lastInput.focus();
	}

	render() {
		return html`
			${this._renderItems()}
			<uui-button id="action" label="Add" look="placeholder" color="default" @click="${this._onAdd}"></uui-button>
		`;
	}

	private _renderItems() {
		return html` ${repeat(
			this._value,
			(item, index) => index,
			(item, index) => html`${this._renderItem(item, index)}`
		)}`;
	}

	private _renderItem(item: MultipleTextStringValueItem, index: number) {
		return html`<div class="item">
			<uui-icon name="umb:navigation"></uui-icon>
			<uui-input
				id="text-field"
				value="${item.value}"
				data-value-index="${index}"
				@input="${this._onInput}"></uui-input>
			<uui-button label="Hello" look="primary" color="danger" compact>
				<uui-icon name="umb:trash"></uui-icon>
			</uui-button>
		</div>`;
	}
}

export default UmbPropertyEditorUIMultipleTextStringElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-multiple-text-string': UmbPropertyEditorUIMultipleTextStringElement;
	}
}
