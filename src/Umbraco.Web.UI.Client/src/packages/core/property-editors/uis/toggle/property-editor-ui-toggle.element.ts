import { html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { UmbInputToggleElement } from '../../../components/input-toggle/input-toggle.element';
import { UmbPropertyEditorExtensionElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { DataTypePropertyPresentationModel } from '@umbraco-cms/backoffice/backend-api';

/**
 * @element umb-property-editor-ui-toggle
 */
@customElement('umb-property-editor-ui-toggle')
export class UmbPropertyEditorUIToggleElement extends UmbLitElement implements UmbPropertyEditorExtensionElement {
	@property()
	value = false;

	@state()
	_labelOff?: string;

	@state()
	_labelOn?: string;

	@state()
	_showLabels?: boolean;

	@property({ type: Array, attribute: false })
	public set config(config: Array<DataTypePropertyPresentationModel>) {
		const defaultValue = config.find((x) => x.alias === 'default');
		if (defaultValue) this.value = defaultValue.value as boolean;

		const labelOff = config.find((x) => x.alias === 'labelOff');
		if (labelOff) this._labelOff = labelOff.value as string;

		const labelOn = config.find((x) => x.alias === 'labelOn');
		if (labelOn) this._labelOn = labelOn.value as string;

		const showLabels = config.find((x) => x.alias === 'showLabels');
		if (showLabels) this._showLabels = showLabels.value as boolean;
	}

	private _onChange(event: CustomEvent) {
		this.value = (event.target as UmbInputToggleElement).checked;
		this.dispatchEvent(new CustomEvent('property-value-change'));
	}

	render() {
		return html`<umb-input-toggle
			?checked="${this.value}"
			.labelOn="${this._labelOn}"
			.labelOff=${this._labelOff}
			?showLabels="${this._showLabels}"
			@change="${this._onChange}"></umb-input-toggle>`;
	}

	static styles = [UUITextStyles];
}

export default UmbPropertyEditorUIToggleElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-toggle': UmbPropertyEditorUIToggleElement;
	}
}
