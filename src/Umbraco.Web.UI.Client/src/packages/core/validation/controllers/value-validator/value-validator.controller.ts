import { UMB_VALIDATION_CONTEXT } from '../../context/validation.context-token.js';
import type { UmbValidator } from '../../interfaces/validator.interface.js';
import { UMB_VALIDATION_EMPTY_LOCALIZATION_KEY } from '../../const.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

export interface UmbValueValidatorArgs<ValueType = unknown> {
	dataPath?: string;
	check?: (value: ValueType) => boolean;
	message?: () => string;
	navigateToError?: () => void;
}

/**
 * UmbValueValidator is a controller that implements the UmbValidator interface.
 * It validates a value based on a provided check function, manages validation state,
 * and communicates validation messages to the validation context.
 * It can also handle navigation to the first invalid element if needed.
 */
export class UmbValueValidator<ValueType = unknown> extends UmbControllerBase implements UmbValidator {
	//
	#validationContext?: typeof UMB_VALIDATION_CONTEXT.TYPE;
	#validationMode: boolean = false;

	#dataPath: undefined | string;
	#check: (value: ValueType) => boolean;
	#message: () => string;
	#navigateToError?: () => void;

	#isValid: boolean = false;
	#value?: ValueType;

	set value(value: ValueType | undefined) {
		this.#value = value;
		this.runCheck();
	}
	get value(): ValueType | undefined {
		return this.#value;
	}

	constructor(host: UmbControllerHost, args: UmbValueValidatorArgs<ValueType>) {
		super(host);
		this.#dataPath = args.dataPath;
		this.#check =
			args.check ??
			((value: ValueType) => {
				return value === undefined || value === null || value === '';
			});
		this.#message = args.message ?? (() => UMB_VALIDATION_EMPTY_LOCALIZATION_KEY);
		this.#navigateToError = args.navigateToError;

		this.consumeContext(UMB_VALIDATION_CONTEXT, (context) => {
			if (this.#validationContext !== context) {
				this.#validationContext?.removeValidator(this);
				this.#validationContext = context;
				this.#validationContext?.addValidator(this);
				this.runCheck();
			}
		});
	}

	runCheck() {
		if (!this.#validationMode) return;
		// Check validation:
		this.#isValid = !this.#check(this.#value as ValueType);

		// Update validation message:
		if (this.#validationContext) {
			if (this.#dataPath) {
				if (this.#isValid) {
					this.#validationContext.messages.removeMessagesByTypeAndPath('custom', this.#dataPath);
				} else {
					this.#validationContext.messages.addMessage('custom', this.#dataPath, this.#message());
				}
			}
		}
	}

	async validate(): Promise<void> {
		this.#validationMode = true;
		// Validate is called when the validation state of this validator is asked to be fully resolved. Like when user clicks Save & Publish.
		// If you need to ask the server then it could be done here, instead of asking the server each time the value changes.
		// In this particular example we do not need to do anything, because our validation is represented via a message that we always set no matter the user interaction.
		// If we did not like to only to check the Validation State when absolute needed then this method must be implemented.
		return this.runCheck();
	}

	get isValid(): boolean {
		return this.#isValid;
	}

	reset(): void {
		this.#isValid = false;
		this.#validationMode = false;
	}

	/**
	 * Focus the first invalid element.
	 */
	focusFirstInvalidElement(): void {
		this.#navigateToError?.();
	}

	override destroy(): void {
		if (!this.#isValid && this.#dataPath) {
			this.#validationContext?.messages.removeMessagesByTypeAndPath('custom', this.#dataPath);
		}
		this.#validationContext = undefined;
		this.#value = undefined;
		super.destroy();
	}
}
