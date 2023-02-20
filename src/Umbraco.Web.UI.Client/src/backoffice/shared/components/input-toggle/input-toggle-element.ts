import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { FormControlMixin } from '@umbraco-ui/uui-base/lib/mixins';
import { UUIBooleanInputEvent } from '@umbraco-ui/uui';
import { UmbLitElement } from '@umbraco-cms/element';

@customElement('umb-input-toggle')
export class UmbInputToggleElement extends FormControlMixin(UmbLitElement) {
	static styles = [
		UUITextStyles,
		css`
			uui-toggle {
				width: 100%;
			}
		`,
	];

	_checked = false;
	@property({ type: Boolean })
	public set checked(toggle: boolean) {
		this._checked = toggle;
		super.value = toggle.toString();
		this.#updateLabel();
	}
	public get checked(): boolean {
		return this._checked;
	}

	@property({ type: Boolean })
	showLabels = false;

	@property({ type: String })
	labelOn?: string;

	@property({ type: String })
	labelOff?: string;

	@state()
	_currentLabel?: string;

	protected getFormElement() {
		return undefined;
	}

	constructor() {
		super();
	}

	connectedCallback(): void {
		super.connectedCallback();
		this.#updateLabel();
	}

	#onChange(e: UUIBooleanInputEvent) {
		this.checked = e.target.checked;
		e.stopPropagation();
		this.dispatchEvent(new CustomEvent('change', { bubbles: true, composed: true }));
	}

	#updateLabel() {
		this._currentLabel = this.showLabels ? (this.checked ? this.labelOn : this.labelOff) : '';
	}

	render() {
		return html`<uui-toggle
			.checked="${this._checked}"
			.label="${this._currentLabel}"
			@change="${this.#onChange}"></uui-toggle>`;
	}
}

export default UmbInputToggleElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-toggle': UmbInputToggleElement;
	}
}
