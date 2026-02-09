import type { UmbTogglePropertyEditorUiValue } from './types.js';
import { customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbFormControlMixin, UMB_VALIDATION_FALSE_LOCALIZATION_KEY } from '@umbraco-cms/backoffice/validation';
import type { UmbInputToggleElement } from '@umbraco-cms/backoffice/components';
import type {
	UmbPropertyEditorConfigCollection,
	UmbPropertyEditorUiElement,
} from '@umbraco-cms/backoffice/property-editor';

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
	private _ariaLabel?: string;

	@state()
	private _labelOff?: string;

	@state()
	private _labelOn?: string;

	@state()
	private _showLabels = false;

	public set config(config: UmbPropertyEditorConfigCollection | undefined) {
		if (!config) return;

		this._showLabels = Boolean(config.getValueByAlias('showLabels'));
		this._labelOn = this.localize.string(config.getValueByAlias<string>('labelOn') ?? '');
		this._labelOff = this.localize.string(config.getValueByAlias<string>('labelOff') ?? '');

		this._ariaLabel =
			this.localize.string(config.getValueByAlias<string>('ariaLabel')) ||
			this.localize.term('general_toggleFor', [this.name]);
	}

	protected override firstUpdated(): void {
		this.addFormControlElement(this.shadowRoot!.querySelector('umb-input-toggle')!);
	}

	#onChange(event: CustomEvent & { target: UmbInputToggleElement }) {
		//checked is never null/undefined
		this.value = event.target.checked;
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		return html`
			<umb-input-toggle
				.ariaLabel=${this._ariaLabel ?? null}
				.labelOn=${this._labelOn}
				.labelOff=${this._labelOff}
				.requiredMessage=${this.mandatoryMessage}
				.showLabels=${this._showLabels}
				?checked=${this.value}
				?readonly=${this.readonly}
				?required=${this.mandatory}
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
