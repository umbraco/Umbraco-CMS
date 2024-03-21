import { UmbValidationInvalidEvent } from '../events/validation-invalid.event.js';
import { UmbValidationValidEvent } from '../events/validation-valid.event.js';
import { property, type LitElement } from '@umbraco-cms/backoffice/external/lit';
import type { HTMLElementConstructor } from '@umbraco-cms/backoffice/extension-api';

type NativeFormControlElement = HTMLInputElement | HTMLTextAreaElement | HTMLFormElement; // Eventually use a specific interface or list multiple options like appending these types: ... | HTMLTextAreaElement | HTMLSelectElement

/* FlagTypes type options originate from:
 * https://developer.mozilla.org/en-US/docs/Web/API/ValidityState
 * */
type FlagTypes =
	| 'badInput'
	| 'customError'
	| 'patternMismatch'
	| 'rangeOverflow'
	| 'rangeUnderflow'
	| 'stepMismatch'
	| 'tooLong'
	| 'tooShort'
	| 'typeMismatch'
	| 'valueMissing'
	| 'badInput'
	| 'valid';

// Acceptable as an internal interface/type, BUT if exposed externally this should be turned into a public class in a separate file.
interface Validator {
	flagKey: FlagTypes;
	getMessageMethod: () => string;
	checkMethod: () => boolean;
}

export interface UmbFormControlMixinInterface<ValueType, DefaultValueType> extends HTMLElement {
	formAssociated: boolean;
	get value(): ValueType | DefaultValueType;
	set value(newValue: ValueType | DefaultValueType);
	name: string;
	formResetCallback(): void;
	checkValidity(): boolean;
	get validationMessage(): string;
	get validity(): ValidityState;
	setCustomValidity(error: string): void;
	submit(): void;
	pristine: boolean;
	required: boolean;
	requiredMessage: string;
	error: boolean;
	errorMessage: string;
}

export declare abstract class UmbFormControlMixinElement<ValueType, DefaultValueType>
	extends HTMLElement
	implements UmbFormControlMixinInterface<ValueType, DefaultValueType>
{
	protected _internals: ElementInternals;
	protected _runValidators(): void;
	protected abstract getFormElement(): HTMLElement | undefined;
	protected addValidator: (flagKey: FlagTypes, getMessageMethod: () => string, checkMethod: () => boolean) => void;
	protected addFormControlElement(element: NativeFormControlElement): void;

	formAssociated: boolean;
	get value(): ValueType | DefaultValueType;
	set value(newValue: ValueType | DefaultValueType);
	name: string;
	formResetCallback(): void;
	checkValidity(): boolean;
	get validationMessage(): string;
	get validity(): ValidityState;
	setCustomValidity(error: string): void;
	submit(): void;
	pristine: boolean;
	required: boolean;
	requiredMessage: string;
	error: boolean;
	errorMessage: string;
}

/**
 * The mixin allows a custom element to participate in HTML forms.
 *
 * @param {Object} superClass - superclass to be extended.
 * @mixin
 */
export const UUIFormControlMixin = <
	ValueType = FormDataEntryValue | FormData,
	T extends HTMLElementConstructor<LitElement> = HTMLElementConstructor<LitElement>,
	DefaultValueType = unknown,
>(
	superClass: T,
	defaultValue: DefaultValueType,
) => {
	abstract class UmbFormControlMixinClass extends superClass {
		/**
		 * This is a static class field indicating that the element is can be used inside a native form and participate in its events.
		 * It may require a polyfill, check support here https://developer.mozilla.org/en-US/docs/Web/API/HTMLElement/attachInternals.
		 * Read more about form controls here https://web.dev/more-capable-form-controls/
		 * @type {boolean}
		 */
		static readonly formAssociated = true;

		/**
		 * This is a name property of the component.
		 * @type {string}
		 * @attr
		 * @default ''
		 */
		@property({ type: String })
		name = '';

		/**
		 * Value of this form control.
		 * If you dont want the setFormValue to be called on the ElementInternals, then prevent calling this method, by not calling super.value = newValue in your implementation of the value setter method.
		 * @type {string}
		 * @attr value
		 * @default ''
		 */
		@property() // Do not 'reflect' as the attribute is used as fallback.
		get value(): ValueType | DefaultValueType {
			return this.#value;
		}
		set value(newValue: ValueType | DefaultValueType) {
			const oldValue = this.#value;
			this.#value = newValue;
			if ('ElementInternals' in window && 'setFormValue' in window.ElementInternals.prototype) {
				this._internals.setFormValue((this.#value as any) ?? null);
			}
			this.requestUpdate('value', oldValue);
		}

		// Validation
		private _validityState: any = {};

		/**
		 * Determines wether the form control has been touched or interacted with, this determines wether the validation-status of this form control should be made visible.
		 * @type {boolean}
		 * @attr
		 * @default false
		 */
		@property({ type: Boolean, reflect: true })
		pristine: boolean = true;

		/**
		 * Apply validation rule for requiring a value of this form control.
		 * @attr
		 * @default false
		 */
		@property({ type: Boolean, reflect: true })
		required = false;

		/**
		 * Required validation message.
		 * @attr
		 */
		@property({ type: String, attribute: 'required-message' })
		requiredMessage = 'This field is required';

		/**
		 * Apply custom error on this input.
		 * @attr
		 */
		@property({ type: Boolean, reflect: true })
		error = false;

		/**
		 * Custom error message.
		 * @attr
		 */
		@property({ type: String, attribute: 'error-message' })
		errorMessage = 'This field is invalid';

		#value: ValueType | DefaultValueType = defaultValue;
		_internals: ElementInternals;
		#form: HTMLFormElement | null = null;
		#validators: Validator[] = [];
		#formCtrlElements: NativeFormControlElement[] = [];

		constructor(...args: any[]) {
			super(...args);
			this._internals = this.attachInternals();

			this.addValidator(
				'valueMissing',
				() => this.requiredMessage,
				() => this.hasAttribute('required') && this.hasValue() === false,
			);
			this.addValidator(
				'customError',
				() => this.errorMessage,
				() => this.error,
			);

			this.addEventListener('blur', () => {
				this.pristine = false;
			});
		}

		/**
		 * Determine wether this FormControl has a value.
		 * @method hasValue
		 * @returns {boolean}
		 */
		public hasValue(): boolean {
			return this.value !== this.getDefaultValue();
		}

		/**
		 * Get internal form element.
		 * This has to be implemented to provide a FormControl Element of choice for the given context. The element is used as anchor for validation-messages.
		 * @abstract
		 * @method getFormElement
		 * @returns {HTMLElement | undefined}
		 */
		protected abstract getFormElement(): HTMLElement | undefined;

		disconnectedCallback(): void {
			super.disconnectedCallback();
			this.#removeFormListeners();
		}
		#removeFormListeners() {
			if (this.#form) {
				this.#form.removeEventListener('submit', this.#onFormSubmit);
			}
		}

		/**
		 * Add validator, to validate this Form Control.
		 * See https://developer.mozilla.org/en-US/docs/Web/API/ValidityState for available Validator FlagTypes.
		 *
		 * @example
		 * this.addValidator(
		 *  'tooLong',
		 *  () => 'This input contains too many characters',
		 *  () => this._value.length > 10
		 * );
		 * @method hasValue
		 * @param {FlagTypes} flagKey the type of validation.
		 * @param {method} getMessageMethod method to retrieve relevant message. Is executed every time the validator is re-executed.
		 * @param {method} checkMethod method to determine if this validator should invalidate this form control. Return true if this should prevent submission.
		 */
		protected addValidator(flagKey: FlagTypes, getMessageMethod: () => string, checkMethod: () => boolean): Validator {
			const obj = {
				flagKey: flagKey,
				getMessageMethod: getMessageMethod,
				checkMethod: checkMethod,
			};
			this.#validators.push(obj);
			return obj;
		}

		protected removeValidator(validator: Validator) {
			const index = this.#validators.indexOf(validator);
			if (index !== -1) {
				this.#validators.splice(index, 1);
			}
		}

		/**
		 * @method addFormControlElement
		 * @description Important notice if adding a native form control then ensure that its value and thereby validity is updated when value is changed from the outside.
		 * @param element {NativeFormControlElement} - element to validate and include as part of this form association.
		 */
		protected addFormControlElement(element: NativeFormControlElement) {
			this.#formCtrlElements.push(element);
		}

		private _customValidityObject?: Validator;

		/**
		 * @method setCustomValidity
		 * @description Set custom validity state, set to empty string to remove the custom message.
		 * @param message {string} - The message to be shown
		 * @see {@link https://developer.mozilla.org/en-US/docs/Web/API/HTMLObjectElement/setCustomValidity|HTMLObjectElement:setCustomValidity}
		 */
		protected setCustomValidity(message: string | null) {
			if (this._customValidityObject) {
				this.removeValidator(this._customValidityObject);
			}

			if (message != null && message !== '') {
				this._customValidityObject = this.addValidator(
					'customError',
					(): string => message,
					() => true,
				);
			}

			this._runValidators();
		}

		/**
		 * @method _runValidators
		 * @description Run all validators and set the validityState of this form control.
		 * Run this method when you want to re-run all validators.
		 * This can be relevant if you have a validators that is using values that is not triggering the Lit Updated Callback.
		 * Such are mainly properties that are not declared as a Lit state and or Lit property.
		 */
		protected _runValidators() {
			this._validityState = {};

			// Loop through inner native form controls to adapt their validityState.
			this.#formCtrlElements.forEach((formCtrlEl) => {
				for (const key in formCtrlEl.validity) {
					if (key !== 'valid' && (formCtrlEl.validity as any)[key]) {
						(this as any)._validityState[key] = true;
						this._internals.setValidity((this as any)._validityState, formCtrlEl.validationMessage, formCtrlEl);
					}
				}
			});

			// Loop through custom validators, currently its intentional to have them overwritten native validity. but might need to be reconsidered (This current way enables to overwrite with custom messages) [NL]
			this.#validators.forEach((validator) => {
				if (validator.checkMethod()) {
					this._validityState[validator.flagKey] = true;
					this._internals.setValidity(this._validityState, validator.getMessageMethod(), this.getFormElement());
				}
			});

			const hasError = Object.values(this._validityState).includes(true);

			// https://developer.mozilla.org/en-US/docs/Web/API/ValidityState#valid
			this._validityState.valid = !hasError;

			if (hasError) {
				this.dispatchEvent(new UmbValidationInvalidEvent());
			} else {
				this._internals.setValidity({});
				this.dispatchEvent(new UmbValidationValidEvent());
			}
		}

		updated(changedProperties: Map<string | number | symbol, unknown>) {
			super.updated(changedProperties);
			this._runValidators();
		}

		#onFormSubmit = () => {
			this.pristine = false;
		};

		public submit() {
			this.#form?.requestSubmit();
		}

		public formAssociatedCallback() {
			this.#removeFormListeners();
			this.#form = this._internals.form;
			if (this.#form) {
				// This relies on the form begin a 'uui-form': [NL]
				if (this.#form.hasAttribute('submit-invalid')) {
					this.pristine = false;
				}
				this.#form.addEventListener('submit', this.#onFormSubmit);
			}
		}
		public formResetCallback() {
			this.pristine = true;
			this.value = this.getInitialValue() ?? this.getDefaultValue();
		}

		protected getDefaultValue(): DefaultValueType {
			return defaultValue;
		}
		protected getInitialValue(): ValueType | DefaultValueType {
			return this.getAttribute('value') as ValueType | DefaultValueType;
		}

		public checkValidity() {
			for (const key in this.#formCtrlElements) {
				if (this.#formCtrlElements[key].checkValidity() === false) {
					return false;
				}
			}

			return this._internals?.checkValidity();
		}

		// https://developer.mozilla.org/en-US/docs/Web/API/HTMLObjectElement/validity
		public get validity(): ValidityState {
			return this._validityState;
		}

		get validationMessage() {
			return this._internals?.validationMessage;
		}
	}
	return UmbFormControlMixinClass as unknown as HTMLElementConstructor<
		UmbFormControlMixinElement<ValueType, DefaultValueType>
	> &
		T;
};
