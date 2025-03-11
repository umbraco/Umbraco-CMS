import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import type { UmbTogglePropertyEditorUiValue } from './types.js';
import type { UmbInputToggleElement } from '@umbraco-cms/backoffice/components';
import { customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type {
	UmbPropertyEditorConfigCollection,
	UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/property-editor';
import { UMB_VALIDATION_FALSE_LOCALIZATION_KEY, UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';

@customElement('umb-property-editor-ui-toggle')
export class UmbPropertyEditorUIToggleElement
	extends UmbFormControlMixin<UmbTogglePropertyEditorUiValue, typeof UmbLitElement, undefined>(UmbLitElement)
	implements UmbPropertyEditorUiElement
{
	@property({ type: String })
	name?: string;

	/**
	 * Sets the input to readonly mode, meaning value cannot be changed but still able to read and select its content.
	 * @type {boolean}
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	readonly = false;

	/**
	 * Sets the input to mandatory, meaning validation will fail if the value is empty.
	 * @type {boolean}
	 */
	@property({ type: Boolean })
	mandatory?: boolean;
	@property({ type: String })
	mandatoryMessage = UMB_VALIDATION_FALSE_LOCALIZATION_KEY;

	@state()
	_ariaLabel?: string;

	@state()
	_labelOff?: string;

	@state()
	_labelOn?: string;

	@state()
	_showLabels = false;

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;
		this._labelOff = config.getValueByAlias('labelOff');
		this._labelOn = config.getValueByAlias('labelOn');
		this._showLabels = Boolean(config.getValueByAlias('showLabels'));
		this._ariaLabel = config.getValueByAlias('ariaLabel');
	}

	protected override firstUpdated(): void {
		this.addFormControlElement(this.shadowRoot!.querySelector('umb-input-toggle')!);
	}

	#onChange(event: CustomEvent & { target: UmbInputToggleElement }) {
		const checked = event.target.checked;
		this.value = this.mandatory ? (checked ?? null) : checked;
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return html`
			<umb-input-toggle
				.ariaLabel=${this._ariaLabel
					? this.localize.string(this._ariaLabel)
					: this.localize.term('general_toggleFor', [this.name])}
				.labelOn=${this._labelOn}
				.labelOff=${this._labelOff}
				?checked=${this.value}
				?showLabels=${this._showLabels}
				?required=${this.mandatory}
				.requiredMessage=${this.mandatoryMessage}
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
