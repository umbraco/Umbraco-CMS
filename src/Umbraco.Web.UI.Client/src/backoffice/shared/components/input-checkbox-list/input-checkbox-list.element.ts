import { css, html, nothing } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { FormControlMixin } from '@umbraco-ui/uui-base/lib/mixins';
import { repeat } from 'lit/directives/repeat.js';
import { UUIBooleanInputEvent } from '@umbraco-ui/uui';
import { UmbLitElement } from '@umbraco-cms/element';

@customElement('umb-input-checkbox-list')
export class UmbInputCheckboxListElement extends FormControlMixin(UmbLitElement) {
	static styles = [
		UUITextStyles,
		css`
			uui-checkbox {
				width: 100%;
			}
		`,
	];

	/**
	 * List of items.
	 */
	@property()
	list?: [];

	private _selectedKeys: Array<string> = [];
	public get selectedKeys(): Array<string> {
		return this._selectedKeys;
	}
	public set selectedKeys(keys: Array<string>) {
		this._selectedKeys = keys;
		super.value = keys.join(',');
	}

	@property()
	public set value(keysString: string) {
		if (keysString !== this._value) {
			this.selectedKeys = keysString.split(/[ ,]+/);
		}
	}

	protected getFormElement() {
		return undefined;
	}

	private _setSelection(e: UUIBooleanInputEvent) {
		e.stopPropagation();
		if (e.target.checked) this.selectedKeys = [...this.selectedKeys, e.target.value];
		else this._removeFromSelection(this.selectedKeys.findIndex((key) => e.target.value === key));

		this.dispatchEvent(new CustomEvent('change', { bubbles: true, composed: true }));
	}

	private _removeFromSelection(index: number) {
		if (index == -1) return;
		const keys = [...this.selectedKeys];
		keys.splice(index, 1);
		this.selectedKeys = keys;
	}

	render() {
		if (!this.list) return nothing;
		return html`<form>
			<uui-form @change="${this._setSelection}">
				${repeat(this.list, (item) => item.key, this.renderCheckbox)}
			</uui-form>
		</form>`;
	}

	renderCheckbox(item: any) {
		return html`<uui-checkbox value="${item.key}" label="${item.label}"></uui-checkbox>`;
	}
}

export default UmbInputCheckboxListElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-checkbox-list': UmbInputCheckboxListElement;
	}
}
