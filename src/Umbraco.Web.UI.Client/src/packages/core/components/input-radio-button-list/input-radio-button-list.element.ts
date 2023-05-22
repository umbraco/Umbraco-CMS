import { css, html, nothing } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css';
import { customElement, property } from 'lit/decorators.js';
import { FormControlMixin } from '@umbraco-ui/uui-base/lib/mixins';
import { repeat } from 'lit/directives/repeat.js';
import { UUIBooleanInputEvent } from '@umbraco-ui/uui';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-input-radio-button-list')
export class UmbInputRadioButtonListElement extends FormControlMixin(UmbLitElement) {
	/**
	 * List of items.
	 */
	@property()
	public list: Array<{ key: string; sortOrder: number; value: string }> = [];

	#selected = '';
	public get selected(): string {
		return this.#selected;
	}
	public set selected(key: string) {
		this.#selected = key;
		super.value = key;
	}

	@property()
	public set value(key: string) {
		if (key !== this._value) {
			this.selected = key;
		}
	}

	protected getFormElement() {
		return undefined;
	}

	#setSelection(e: UUIBooleanInputEvent) {
		e.stopPropagation();
		if (e.target.value) this.selected = e.target.value;
		this.dispatchEvent(new CustomEvent('change', { bubbles: true, composed: true }));
	}

	render() {
		if (!this.list) return nothing;

		return html`<uui-radio-group .value=${this.value} @change=${this.#setSelection}>
			${repeat(this.list, (item) => item, this.renderRadioButton)}
		</uui-radio-group>`;
	}

	renderRadioButton(item: { key: string; sortOrder: number; value: string }) {
		return html`<uui-radio value="${item.value}" label="${item.value}"></uui-radio>`;
	}

	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
			}
		`,
	];
}

export default UmbInputRadioButtonListElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-radio-button-list': UmbInputRadioButtonListElement;
	}
}
