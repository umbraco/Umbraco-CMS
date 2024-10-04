import { css, html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UUIFormControlMixin } from '@umbraco-cms/backoffice/external/uui';
import type { UUIBooleanInputEvent } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-input-toggle')
export class UmbInputToggleElement extends UUIFormControlMixin(UmbLitElement, '') {
	@property({ type: Boolean })
	public set checked(toggle: boolean) {
		this.#checked = toggle;
		super.value = toggle.toString();
		this.#updateLabel();
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

	/**
	 * Sets the input to readonly mode, meaning value cannot be changed but still able to read and select its content.
	 * @type {boolean}
	 * @attr
	 * @default false
	 */
	@property({ type: Boolean, reflect: true })
	readonly = false;

	@state()
	_currentLabel?: string;

	protected override getFormElement() {
		return undefined;
	}

	override connectedCallback(): void {
		super.connectedCallback();
		this.#updateLabel();
	}

	#onChange(event: UUIBooleanInputEvent) {
		event.stopPropagation();
		this.checked = event.target.checked;
		this.dispatchEvent(new UmbChangeEvent());
	}

	#updateLabel() {
		this._currentLabel = this.showLabels ? (this.checked ? this.labelOn : this.labelOff) : '';
	}

	override render() {
		return html`<uui-toggle
			.checked=${this.#checked}
			.label=${this._currentLabel}
			@change=${this.#onChange}
			?readonly=${this.readonly}></uui-toggle>`;
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
