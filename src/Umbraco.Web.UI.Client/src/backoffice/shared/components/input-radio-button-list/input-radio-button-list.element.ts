import { css, html, nothing } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';
import { FormControlMixin } from '@umbraco-ui/uui-base/lib/mixins';
import { repeat } from 'lit/directives/repeat.js';
import { UUIBooleanInputEvent } from '@umbraco-ui/uui';
import { UmbLitElement } from '@umbraco-cms/element';

@customElement('umb-input-radio-button-list')
export class UmbInputRadioButtonListElement extends FormControlMixin(UmbLitElement) {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
			}
		`,
	];

	/**
	 * List of items.
	 */
	@property()
	list?: [];

	private _selectedKey = '';
	public get selectedKey(): string {
		return this._selectedKey;
	}
	public set selectedKey(key: string) {
		this._selectedKey = key;
		super.value = key;
	}

	@property()
	public set value(key: string) {
		if (key !== this._value) {
			this.selectedKey = key;
		}
	}

	protected getFormElement() {
		return undefined;
	}

	private _setSelection(e: UUIBooleanInputEvent) {
		e.stopPropagation();
		if (e.target.value) this.selectedKey = e.target.value;
		this.dispatchEvent(new CustomEvent('change', { bubbles: true, composed: true }));
	}

	render() {
		if (!this.list) return nothing;

		return html`<uui-radio-group .value=${this.value} @change=${this._setSelection}>
			${repeat(this.list, (item) => item.key, this.renderRadioButton)}
		</uui-radio-group>`;
	}

	renderRadioButton(item: { key: string; label: string }) {
		return html`<uui-radio value="${item.key}" label="${item.label}"></uui-radio>`;
	}
}

export default UmbInputRadioButtonListElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-radio-button-list': UmbInputRadioButtonListElement;
	}
}
