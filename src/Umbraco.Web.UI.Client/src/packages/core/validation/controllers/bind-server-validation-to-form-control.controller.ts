import type { UmbValidationMessage } from '../context/validation-messages.manager.js';
import { UMB_VALIDATION_CONTEXT } from '../context/validation.context-token.js';
import type { UmbFormControlMixinInterface } from '../mixins/form-control.mixin.js';
import { defaultMemoization, simpleHashCode } from '@umbraco-cms/backoffice/observable-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

const observeSymbol = Symbol();

/**
 * Binds server validation to a form control.
 * This controller will add a custom error to the form control if the validation context has any messages for the specified data path.
 */
export class UmbBindServerValidationToFormControl extends UmbControllerBase {
	#context?: typeof UMB_VALIDATION_CONTEXT.TYPE;

	#control: UmbFormControlMixinInterface<unknown>;

	#controlValidator?: ReturnType<UmbFormControlMixinInterface<unknown>['addValidator']>;
	#messages: Array<UmbValidationMessage> = [];
	#isValid?: boolean;
	// Keep track of the value set, so we can determine if we should remove server validation when the value changes. [NL]
	#gotValue = false;

	#value?: unknown;
	set value(value: unknown) {
		if (this.#isValid || !this.#gotValue) {
			// If valid, or set value set, lets just parse it on [NL]
			this.#value = value;
			this.#gotValue = true;
		} else {
			// If not valid lets see if we should remove server validation [NL]
			if (!defaultMemoization(this.#value, value) && this.#isValid === false) {
				this.#value = value;
				// Only remove server validations from validation context [NL]
				const toRemove = this.#messages.filter((x) => x.type === 'server').map((msg) => msg.key);
				this.#context?.messages?.removeMessageByKeys(toRemove);
			}
		}
	}

	constructor(host: UmbControllerHost, formControl: UmbFormControlMixinInterface<unknown>, dataPath: string) {
		super(host, 'umbFormControlValidation_' + simpleHashCode(dataPath));
		this.#control = formControl;
		this.consumeContext(UMB_VALIDATION_CONTEXT, (context) => {
			this.#context = context;

			this.observe(
				context?.messages?.messagesOfNotTypeAndPath('client', dataPath),
				(messages) => {
					this.#messages = messages ?? [];
					this.#isValid = messages === undefined ? undefined : this.#messages.length === 0;
					if (this.#isValid === false) {
						this.#setup();
					} else if (this.#isValid === true) {
						this.#demolish();
					}
				},
				observeSymbol,
			);
		});
	}

	#setup() {
		if (!this.#controlValidator) {
			this.#controlValidator = this.#control.addValidator(
				'customError',
				() => this.#messages.map((x) => x.body).join(', '),
				() => this.#isValid === false,
			);
			//this.#control.addEventListener('change', this.#onControlChange);
		}
		this.#control.checkValidity();
	}

	#demolish() {
		if (!this.#control || !this.#controlValidator) return;

		this.#control.removeValidator(this.#controlValidator);
		//this.#control.removeEventListener('change', this.#onControlChange);
		this.#controlValidator = undefined;
		this.#control.checkValidity();
	}

	validate(): Promise<void> {
		//this.#isValid = this.#control.checkValidity();
		return this.#isValid ? Promise.resolve() : Promise.reject();
	}

	/**
	 * Resets the validation state of this validator.
	 */
	reset(): void {
		this.#isValid = undefined;
		this.#control.pristine = true; // Make sure the control goes back into not-validation-mode/'untouched'/pristine state.
	}

	/*getMessages(): string[] {
		return [this.#control.validationMessage];
	}*/

	focusFirstInvalidElement(): void {
		this.#control.focusFirstInvalidElement();
	}

	override destroy(): void {
		this.#context = undefined;
		// Reset control setup.
		this.#demolish();
		this.#control = undefined as any;
		super.destroy();
	}
}
