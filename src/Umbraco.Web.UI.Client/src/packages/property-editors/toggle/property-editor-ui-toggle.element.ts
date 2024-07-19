import type { UmbInputToggleElement } from '@umbraco-cms/backoffice/components';
import { customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
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
	_showLabels = false;

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;
		this.value ??= config.getValueByAlias('default') ?? false;
		this._labelOff = config.getValueByAlias('labelOff');
		this._labelOn = config.getValueByAlias('labelOn');
		this._showLabels = Boolean(config.getValueByAlias('showLabels'));
	}

	#onChange(event: CustomEvent & { target: UmbInputToggleElement }) {
		this.value = event.target.checked;
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	override render() {
		return html`
			<umb-input-toggle
				.labelOn=${this._labelOn}
				.labelOff=${this._labelOff}
				?checked=${this.value}
				?showLabels=${this._showLabels}
				@change=${this.#onChange}>
			</umb-input-toggle>
		`;
	}
}

export default UmbPropertyEditorUIToggleElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-toggle': UmbPropertyEditorUIToggleElement;
	}
}
