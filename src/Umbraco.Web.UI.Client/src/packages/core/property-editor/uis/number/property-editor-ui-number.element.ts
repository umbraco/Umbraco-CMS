import { css, html, customElement, property, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from "@umbraco-cms/backoffice/style";
import { UmbPropertyEditorExtensionElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { UmbDataTypeConfigCollection } from '@umbraco-cms/backoffice/components';

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

	@property({ attribute: false })
	public set config(config: UmbDataTypeConfigCollection | undefined) {
		this._min = config?.getValueByAlias('min');
		this._max = config?.getValueByAlias('max');
		this._step = config?.getValueByAlias('step');
	}

	private onInput(e: InputEvent) {
		this.value = (e.target as HTMLInputElement).value;
		this.dispatchEvent(new CustomEvent('property-value-change'));
	}

	render() {
		return html`<uui-input
			.value=${this.value ?? 0}
			type="number"
			max="${ifDefined(this._max)}"
			min="${ifDefined(this._min)}"
			step="${ifDefined(this._step)}"
			@input=${this.onInput}></uui-input>`;
	}

	static styles = [
		UmbTextStyles,
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
