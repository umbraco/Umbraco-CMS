import { html } from '@umbraco-cms/backoffice/external/lit';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import type { UmbDataTypePropertyCollection } from '@umbraco-cms/backoffice/components';
import { UmbInputToggleElement } from '../../../components/input-toggle/input-toggle.element.js';
import { UmbPropertyEditorExtensionElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

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
	public set config(config: UmbDataTypePropertyCollection) {
		this.value = config.getValueByAlias('default') ?? false;
		this._labelOff = config.getValueByAlias('labelOff');
		this._labelOn = config.getValueByAlias('labelOn');
		this._showLabels = config.getValueByAlias('showLabels');
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
