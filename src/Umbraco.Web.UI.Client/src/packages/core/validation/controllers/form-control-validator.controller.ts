import type { UmbValidator } from '../interfaces/index.js';
import { UMB_VALIDATION_CONTEXT } from '../context/validation.context-token.js';
import type { UmbFormControlMixinInterface } from '../mixins/form-control.mixin.js';
import { UmbValidationInvalidEvent } from '../events/validation-invalid.event.js';
import { UmbValidationValidEvent } from '../events/validation-valid.event.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * Bind a Form Controls validation state to the validation context.
 * This validator will validate the form control and add messages to the validation context if the form control is invalid.
 */
export class UmbFormControlValidator extends UmbControllerBase implements UmbValidator {
	// The path to the data that this validator is validating.
	readonly #dataPath?: string;

	#context?: typeof UMB_VALIDATION_CONTEXT.TYPE;

	#control: UmbFormControlMixinInterface<unknown>;

	#isValid = true;

	constructor(host: UmbControllerHost, formControl: UmbFormControlMixinInterface<unknown>, dataPath?: string) {
		super(host);
		this.#dataPath = dataPath;
		this.consumeContext(UMB_VALIDATION_CONTEXT, (context) => {
			if (this.#context) {
				this.#context.removeValidator(this);
			}
			this.#context = context;
			context?.addValidator(this);
			// If we have a message already, then un-pristine the control:
			if (dataPath && context?.messages.getHasMessagesOfPathAndDescendant(dataPath)) {
				formControl.pristine = false;
			}
		});
		this.#control = formControl;
	}

	get isValid(): boolean {
		return this.#isValid;
	}
	#setIsValid(newVal: boolean) {
		if (this.#isValid === newVal) return;
		this.#isValid = newVal;

		if (this.#dataPath) {
			if (newVal) {
				this.#context?.messages.removeMessagesByTypeAndPath('client', this.#dataPath);
			} else {
				// We only want to add the message if it is not already there. (this could be a custom or server message that got binded to the control, we do not want that double.)
				if (!this.#context?.messages.getHasMessageOfPathAndBody(this.#dataPath, this.#control.validationMessage)) {
					this.#context?.messages.addMessage('client', this.#dataPath, this.#control.validationMessage);
				}
			}
		}
		//this.dispatchEvent(new CustomEvent('change')); // To let the ValidationContext know that the validation state has changed.
	}

	#setInvalid = this.#setIsValid.bind(this, false);
	#setValid = this.#setIsValid.bind(this, true);

	validate(): Promise<void> {
		this.#isValid = this.#control.checkValidity();
		return this.#isValid ? Promise.resolve() : Promise.reject();
	}

	/**
	 * Resets the validation state of this validator.
	 */
	reset(): void {
		this.#isValid = false;
		this.#control.pristine = true; // Make sure the control goes back into not-validation-mode/'untouched'/pristine state.
	}

	/*getMessages(): string[] {
		return [this.#control.validationMessage];
	}*/

	focusFirstInvalidElement(): void {
		this.#control.focusFirstInvalidElement();
	}

	override hostConnected(): void {
		super.hostConnected();
		this.#control.addEventListener(UmbValidationInvalidEvent.TYPE, this.#setInvalid);
		this.#control.addEventListener(UmbValidationValidEvent.TYPE, this.#setValid);
		if (this.#context) {
			this.#context.addValidator(this);
		}
	}
	override hostDisconnected(): void {
		super.hostDisconnected();
		if (this.#control) {
			this.#control.removeEventListener(UmbValidationInvalidEvent.TYPE, this.#setInvalid);
			this.#control.removeEventListener(UmbValidationValidEvent.TYPE, this.#setValid);
		}
		if (this.#context) {
			this.#context.removeValidator(this);
			// Remove any messages that this validator has added:
			if (this.#dataPath) {
				//this.#context.messages.removeMessagesByTypeAndPath('client', this.#dataPath);
			}
			this.#context = undefined;
		}
	}

	override destroy(): void {
		super.destroy();
		if (this.#control) {
			this.#control = undefined as any;
		}
	}
}
