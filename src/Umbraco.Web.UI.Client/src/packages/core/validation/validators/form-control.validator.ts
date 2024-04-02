import type { UmbValidator } from '../interfaces/index.js';
import { UMB_VALIDATION_CONTEXT } from '../context/validation.context-token.js';
import type { UmbFormControlMixinInterface } from '../mixins/form-control.mixin.js';
import { UmbValidationInvalidEvent } from '../events/validation-invalid.event.js';
import { UmbValidationValidEvent } from '../events/validation-valid.event.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerAlias, UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbFormControlValidator extends UmbControllerBase implements UmbValidator {
	#context?: typeof UMB_VALIDATION_CONTEXT.TYPE;

	#control: UmbFormControlMixinInterface<unknown, unknown>;
	readonly controllerAlias: UmbControllerAlias;

	#isValid = false;

	constructor(host: UmbControllerHost, formControl: UmbFormControlMixinInterface<unknown, unknown>) {
		super(host);
		this.consumeContext(UMB_VALIDATION_CONTEXT, (context) => {
			if (this.#context) {
				this.#context.removeValidator(this);
			}
			this.#context = context;
			context.addValidator(this);
		});
		this.#control = formControl;
		this.#control.addEventListener(UmbValidationInvalidEvent.TYPE, this.#setInvalid);
		this.#control.addEventListener(UmbValidationValidEvent.TYPE, this.#setValid);
	}

	get isValid(): boolean {
		return this.#isValid;
	}
	#setIsValid(newVal: boolean) {
		if (this.#isValid === newVal) return;
		this.#isValid = newVal;
		this.dispatchEvent(new CustomEvent('change'));
	}

	#setInvalid = this.#setIsValid.bind(this, false);
	#setValid = this.#setIsValid.bind(this, true);

	validate(): Promise<boolean> {
		this.#isValid = this.#control.checkValidity();
		return Promise.resolve(this.#isValid);
	}

	/**
	 * Resets the validation state of this validator.
	 */
	reset(): void {
		this.#isValid = false;
		this.#control.pristine = true; // Make sure the control goes back into not-validation-mode/'untouched'/pristine state.
	}

	getMessages(): string[] {
		return [this.#control.validationMessage];
	}

	focusFirstInvalidElement(): void {
		this.#control.focusFirstInvalidElement();
	}

	destroy(): void {
		if (this.#context) {
			this.#context.removeValidator(this);
			this.#context = undefined;
		}
		if (this.#control) {
			this.#control.removeEventListener(UmbValidationInvalidEvent.TYPE, this.#setInvalid);
			this.#control.removeEventListener(UmbValidationValidEvent.TYPE, this.#setValid);
			this.#control = undefined as any;
		}
		super.destroy();
	}
}
