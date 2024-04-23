import type { UmbInputToggleElement } from '../../core/components/input-toggle/input-toggle.element.js';
import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbPropertyValueChangeEvent } from '@umbraco-cms/backoffice/property-editor';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';

/**
 * @element umb-property-editor-ui-toggle
 */
@customElement('umb-property-editor-ui-toggle')
export class UmbPropertyEditorUIToggleElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@property({ type: Boolean })
	value: undefined | boolean = undefined;

	@state()
	_labelOff?: string;

	@state()
	_labelOn?: string;

	@state()
	_showLabels?: boolean;

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		this.value ??= config?.getValueByAlias('default') ?? false;
		this._labelOff = config?.getValueByAlias('labelOff');
		this._labelOn = config?.getValueByAlias('labelOn');
		this._showLabels = config?.getValueByAlias('showLabels');
	}

	#onChange(event: CustomEvent & { target: UmbInputToggleElement }) {
		this.value = event.target.checked;
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	render() {
		return html`<umb-input-toggle
			?checked="${this.value}"
			.labelOn="${this._labelOn}"
			.labelOff=${this._labelOff}
			?showLabels="${this._showLabels}"
			@change="${this.#onChange}"></umb-input-toggle>`;
	}
}

export default UmbPropertyEditorUIToggleElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-toggle': UmbPropertyEditorUIToggleElement;
	}
}
