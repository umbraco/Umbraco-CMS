import { UmbValidationInvalidEvent } from '../events/validation-invalid.event.js';
import { UmbValidationValidEvent } from '../events/validation-valid.event.js';
import { property, type LitElement } from '@umbraco-cms/backoffice/external/lit';
import type { HTMLElementConstructor } from '@umbraco-cms/backoffice/extension-api';


// Type Definitions
// --------------

type UmbNativeFormControlElement = Pick<
	HTMLObjectElement,
	'validity' | 'checkValidity' | 'validationMessage' | 'setCustomValidity'
> &
	HTMLElement;

export interface UmbFormControlValidatorConfig {
	flagKey: FlagTypes;
	getMessageMethod: () => string;
	checkMethod: () => boolean;
	weight: number;
}

type FlagTypes =
	| 'customError'
	| 'badInput'
	| 'patternMismatch'
	| 'rangeOverflow'
	| 'rangeUnderflow'
	| 'stepMismatch'
	| 'tooLong'
	| 'tooShort'
	| 'typeMismatch'
	| 'valueMissing'
	| 'valid';

const WeightedErrorFlagTypes: FlagTypes[] = [
	'customError', 'valueMissing', 'badInput', 'typeMismatch', 'patternMismatch',
	'rangeOverflow', 'rangeUnderflow', 'stepMismatch', 'tooLong', 'tooShort'
];


// Validator Management
// --------------

class ValidatorManager {
	private validators: UmbFormControlValidatorConfig[] = [];

	addValidator(config: Omit<UmbFormControlValidatorConfig, 'weight'>): UmbFormControlValidatorConfig {
		const validator = { ...config, weight: WeightedErrorFlagTypes.indexOf(config.flagKey) };
		this.validators.push(validator);
		this.sortValidators();
		return validator;
	}

	removeValidator(validator: UmbFormControlValidatorConfig) {
		const index = this.validators.indexOf(validator);
		if (index !== -1) this.validators.splice(index, 1);
	}

	sortValidators() {
		this.validators.sort((a, b) => a.weight - b.weight);
	}

	getFirstFailingValidator(): UmbFormControlValidatorConfig | undefined {
		return this.validators.find(validator => !validator.checkMethod());
	}

	getValidationMessage(): string {
		const failingValidator = this.getFirstFailingValidator();
		return failingValidator ? failingValidator.getMessageMethod() : '';
	}

	checkValidity(): boolean {
		return this.getFirstFailingValidator() === undefined;
	}

	clearValidators() {
		this.validators = [];
	}
}


// Form Control Handling
// --------------

class FormControlManager {
	private formCtrlElements: Set<UmbNativeFormControlElement> = new Set();

	addFormControlElement(element: UmbNativeFormControlElement) {
		this.formCtrlElements.add(element);
	}

	removeFormControlElement(element: UmbNativeFormControlElement) {
		this.formCtrlElements.delete(element);
	}

	findFirstInvalidElement(): UmbNativeFormControlElement | undefined {
		for (const el of this.formCtrlElements) {
			if (!el.validity.valid) return el;
		}
		return undefined;
	}

	checkAllValidity(): boolean {
		for (const el of this.formCtrlElements) {
			if (!el.checkValidity()) return false;
		}
		return true;
	}

	clearFormControls() {
		this.formCtrlElements.clear();
	}
}


// Mixin Definition
// --------------

export const UmbFormControlMixin = <T extends HTMLElementConstructor<LitElement>>(
	superClass: T
) => {
	class UmbFormControlElement extends superClass {
		@property({ type: Boolean, attribute: 'disabled', reflect: true })
		disabled = false;

		validatorManager = new ValidatorManager();
		formControlManager = new FormControlManager();

		connectedCallback() {
			super.connectedCallback();
			this.dispatchEvent(new Event('form-control-ready', { bubbles: true, composed: true }));
		}

		async validate(): Promise<boolean> {
			const validationMessage = this.validatorManager.getValidationMessage();

			if (validationMessage) {
				this.dispatchEvent(new UmbValidationInvalidEvent(validationMessage));
				return false;
			} else {
				this.dispatchEvent(new UmbValidationValidEvent());
				return true;
			}
		}

		checkValidity(): boolean {
			return this.validatorManager.checkValidity() && this.formControlManager.checkAllValidity();
		}

		addValidator(config: Omit<UmbFormControlValidatorConfig, 'weight'>) {
			return this.validatorManager.addValidator(config);
		}

		removeValidator(validator: UmbFormControlValidatorConfig) {
			this.validatorManager.removeValidator(validator);
		}

		clearValidators() {
			this.validatorManager.clearValidators();
		}

		addFormControlElement(element: UmbNativeFormControlElement) {
			this.formControlManager.addFormControlElement(element);
		}

		removeFormControlElement(element: UmbNativeFormControlElement) {
			this.formControlManager.removeFormControlElement(element);
		}

		clearFormControls() {
			this.formControlManager.clearFormControls();
		}
	}

	return UmbFormControlElement;
};
