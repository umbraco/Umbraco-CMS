import { css, html, nothing } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { FormControlMixin } from '@umbraco-ui/uui-base/lib/mixins';
import { UmbLitElement } from '@umbraco-cms/element';

@customElement('umb-eye-dropper')
export class UmbEyeDropperElement extends FormControlMixin(UmbLitElement) {
	static styles = [UUITextStyles];

	constructor() {
		super();
	}

	protected getFormElement() {
		return undefined;
	}

	render() {
		return html`<uui-color-picker></uui-color-picker> `;
	}
}

export default UmbEyeDropperElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-eye-dropper': UmbEyeDropperElement;
	}
}
