import type { UmbInputToggleElement } from '@umbraco-cms/backoffice/components';
import { customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbPropertyValueChangeEvent } from '@umbraco-cms/backoffice/property-editor';
import type {
	UmbPropertyEditorConfigCollection,
	UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/property-editor';

/**
 * @element umb-property-editor-ui-toggle
 */
@customElement('umb-property-editor-ui-toggle')
export class UmbPropertyEditorUIToggleElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	@property({ type: Boolean })
	value: undefined | boolean = undefined;

	/**
	 * Sets the input to readonly mode, meaning value cannot be changed but still able to read and select its content.
	 * @type {boolean}
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	readonly = false;

	@state()
	_ariaLabel?: string;

	@state()
	_labelOff?: string;

	@state()
	_labelOn?: string;

	@property({ type: String })
	name?: string;

	@state()
	_showLabels = false;


	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;
		this.value ??= config.getValueByAlias('default') ?? false;

		this._labelOff = config.getValueByAlias('labelOff');
		this._labelOn = config.getValueByAlias('labelOn');
		this._showLabels = Boolean(config.getValueByAlias('showLabels'));
		this._ariaLabel = config.getValueByAlias('ariaLabel');
	}

	#onChange(event: CustomEvent & { target: UmbInputToggleElement }) {
		this.value = event.target.checked;
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	override render() {
		return html`
			<umb-input-toggle

				.ariaLabel=${this._ariaLabel ? this.localize.string(this._ariaLabel) : this.localize.term('general_toggleFor', [this.name])}
				.labelOff=${this._labelOff}
				?checked=${this.value}
				?showLabels=${this._showLabels}
				@change=${this.#onChange}
				?readonly=${this.readonly}>
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
