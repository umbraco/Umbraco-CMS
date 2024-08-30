import { UmbValidationInvalidEvent, UmbValidationValidEvent } from '../events/index.js';
import type { UmbFormControlMixinInterface } from '../mixins/index.js';
import { css, customElement, html, property, repeat, unsafeHTML } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

/**
 * @description - Component for displaying one or more validation messages from UMB/UUI Form Control within the given scope.
 * Notice: Only supports components that is build on the UMB / UUI FormControlMixing.
 * @slot - for button contents
 * @slot message - for extras in the messages container
 * @see FormControlMixin
 */
@customElement('umb-form-validation-message')
export class UmbFormValidationMessageElement extends UmbLitElement {
	/**
	 * Set the element containing Form Controls of interest.
	 * @type {string}
	 * @default
	 */
	@property({ reflect: false, attribute: true })
	public get for(): HTMLElement | string | null {
		return this._for;
	}
	public set for(value: HTMLElement | string | null) {
		let element = null;
		if (typeof value === 'string') {
			const scope = this.getRootNode();
			element = (scope as DocumentFragment)?.getElementById(value);
		} else if (value instanceof HTMLElement) {
			element = value;
		}
		const newScope = element ?? this;
		const oldScope = this._for;

		if (oldScope === newScope) {
			return;
		}
		if (oldScope !== null) {
			oldScope.removeEventListener(UmbValidationInvalidEvent.TYPE, this.#onControlInvalid as EventListener);
			oldScope.removeEventListener(UmbValidationValidEvent.TYPE, this.#onControlValid as EventListener);
		}
		this._for = newScope;
		this._for.addEventListener(UmbValidationInvalidEvent.TYPE, this.#onControlInvalid as EventListener);
		this._for.addEventListener(UmbValidationValidEvent.TYPE, this.#onControlValid as EventListener);
	}
	private _for: HTMLElement | null = null;

	constructor() {
		super();
		if (this.for === null) {
			this.for = this;
		}
	}

	private _messages = new Map<UmbFormControlMixinInterface<unknown>, string>();

	#onControlInvalid = async (e: UmbValidationInvalidEvent) => {
		const ctrl = (e as any).composedPath()[0];
		if (ctrl.pristine === false) {
			// Currently we only show message from components who does have the pristine property. (we only want to show messages from fields that are NOT pristine aka. that are dirty or in a from that has been submitted)
			// Notice we use the localization controller here, this is different frm the UUI component which uses the same name.
			this._messages.set(ctrl, this.localize.string(ctrl.validationMessage));
		} else {
			this._messages.delete(ctrl);
		}
		this.requestUpdate();
	};

	#onControlValid = (e: UmbValidationValidEvent) => {
		const ctrl = (e as any).composedPath()[0];
		this._messages.delete(ctrl);
		this.requestUpdate();
	};

	override render() {
		return html`
			<slot></slot>
			<div id="messages">
				${repeat(this._messages, (item) => html`<div>${unsafeHTML(item[1])}</div>`)}
				<slot name="message"></slot>
			</div>
		`;
	}

	static override styles = [
		css`
			#messages {
				color: var(--uui-color-danger-standalone);
			}
		`,
	];
}
declare global {
	interface HTMLElementTagNameMap {
		'umb-form-validation-message': UmbFormValidationMessageElement;
	}
}
