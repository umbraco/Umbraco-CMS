import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css';
import { customElement, property } from 'lit/decorators.js';
import { FormControlMixin } from '@umbraco-ui/uui-base/lib/mixins';
import { UUIColorPickerChangeEvent } from '@umbraco-ui/uui';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-input-eye-dropper')
export class UmbInputEyeDropperElement extends FormControlMixin(UmbLitElement) {
	protected getFormElement() {
		return undefined;
	}

	private _onChange(e: UUIColorPickerChangeEvent) {
		e.stopPropagation();
		super.value = e.target.value;
		this.dispatchEvent(new CustomEvent('change'));
	}

	@property({ type: Boolean })
	opacity = false;

	@property()
	swatches: string[] = [];
	//TODO if empty swatches, the color picker still shows the area where they are supposed to  be rendered.
	// BTW in the old backoffice "palette" seemed to be true/false setting, but here its an array.

	render() {
		return html`<uui-color-picker
			label="Eye dropper"
			@change="${this._onChange}"
			.opacity="${this.opacity}"
			.swatches="${this.swatches}"></uui-color-picker>`;
	}

	static styles = [UUITextStyles, css``];
}

export default UmbInputEyeDropperElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-eye-dropper': UmbInputEyeDropperElement;
	}
}
