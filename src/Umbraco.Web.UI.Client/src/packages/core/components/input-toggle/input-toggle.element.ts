import { css, html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UUIBooleanInputEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbFormControlMixin } from '@umbraco-cms/backoffice/validation';

@customElement('umb-input-toggle')
export class UmbInputToggleElement extends UmbFormControlMixin(UmbLitElement, '') {
	/**
	 * Sets the input to required, meaning validation will fail if the value is empty.
	 * @type {boolean}
	 */
	@property({ type: Boolean })
	required?: boolean;

	@property({ type: String })
	requiredMessage?: string;

	@property({ type: Boolean })
	public set checked(toggle: boolean) {
		this.#checked = toggle;
		super.value = toggle.toString();
	}
	public get checked(): boolean {
		return this.#checked;
	}
	#checked = false;

	@property({ type: Boolean })
	showLabels = false;

	@property({ type: String })
	labelOn?: string;

	@property({ type: String })
	labelOff?: string;

	@property({ type: String, attribute: 'aria-label' })
	override ariaLabel: string | null = null;

	/**
	 * Sets the input to readonly mode, meaning value cannot be changed but still able to read and select its content.
	 * @type {boolean}
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	readonly = false;

	@state()
	_currentLabel?: string;

	protected override firstUpdated(): void {
		this.addFormControlElement(this.shadowRoot!.querySelector('uui-toggle')!);
	}

	#onChange(event: UUIBooleanInputEvent) {
		event.stopPropagation();
		this.checked = event.target.checked;
		this.dispatchEvent(new UmbChangeEvent());
	}

	override render() {
		const label = this.showLabels ? (this.checked ? this.labelOn : this.labelOff) : '';
		return html`<uui-toggle
			.checked=${this.#checked}
			.label=${this.ariaLabel}
			?required=${this.required}
			.requiredMessage=${this.requiredMessage}
			@change=${this.#onChange}
			?readonly=${this.readonly}
			><span>${label}</span>
		</uui-toggle>`;
	}

	static override styles = [
		css`
			uui-toggle {
				width: 100%;
			}
		`,
	];
}

export default UmbInputToggleElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-toggle': UmbInputToggleElement;
	}
}
