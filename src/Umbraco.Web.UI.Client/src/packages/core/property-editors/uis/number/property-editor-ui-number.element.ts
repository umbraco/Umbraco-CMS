import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { ifDefined } from 'lit/directives/if-defined.js';
import { UmbPropertyEditorExtensionElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { DataTypePropertyPresentationModel } from '@umbraco-cms/backoffice/backend-api';

@customElement('umb-property-editor-ui-number')
export class UmbPropertyEditorUINumberElement extends UmbLitElement implements UmbPropertyEditorExtensionElement {
	@property()
	value = '';

	@state()
	private _max?: number;

	@state()
	private _min?: number;

	@state()
	private _step?: number;

	@property({ type: Array, attribute: false })
	public set config(config: Array<DataTypePropertyPresentationModel>) {
		const min = config.find((x) => x.alias === 'min');
		if (min) this._min = min.value;

		const max = config.find((x) => x.alias === 'max');
		if (max) this._max = max.value;

		const step = config.find((x) => x.alias === 'step');
		if (step) this._step = step.value;
	}

	private onInput(e: InputEvent) {
		this.value = (e.target as HTMLInputElement).value;
		this.dispatchEvent(new CustomEvent('property-value-change'));
	}

	render() {
		return html`<uui-input
			.value=${this.value}
			type="number"
			max="${ifDefined(this._max)}"
			min="${ifDefined(this._min)}"
			step="${ifDefined(this._step)}"
			@input=${this.onInput}></uui-input>`;
	}

	static styles = [
		UUITextStyles,
		css`
			uui-input {
				width: 100%;
			}
		`,
	];
}

export default UmbPropertyEditorUINumberElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-number': UmbPropertyEditorUINumberElement;
	}
}
