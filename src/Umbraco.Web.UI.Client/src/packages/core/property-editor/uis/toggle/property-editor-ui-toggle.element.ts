import { UmbInputToggleElement } from '../../../components/input-toggle/input-toggle.element.js';
import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from "@umbraco-cms/backoffice/style";
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

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

	@property({ attribute: false })
	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		this.value ??= config?.getValueByAlias('default') ?? false;
		this._labelOff = config?.getValueByAlias('labelOff');
		this._labelOn = config?.getValueByAlias('labelOn');
		this._showLabels = config?.getValueByAlias('showLabels');
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

	static styles = [UmbTextStyles];
}

export default UmbPropertyEditorUIToggleElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-toggle': UmbPropertyEditorUIToggleElement;
	}
}
