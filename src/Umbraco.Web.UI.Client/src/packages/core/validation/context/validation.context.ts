import type { UmbValidator } from '../interfaces/validator.interface.js';
import { UmbValidationMessagesManager } from './validation-messages.manager.js';
import { UMB_VALIDATION_CONTEXT } from './validation.context-token.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbValidationContext extends UmbContextBase<UmbValidationContext> implements UmbValidator {
	#validators: Array<UmbValidator> = [];
	#validationMode: boolean = false;
	#isValid: boolean = false;

	public readonly messages = new UmbValidationMessagesManager();

	constructor(host: UmbControllerHost) {
		super(host, UMB_VALIDATION_CONTEXT);
	}

	get isValid(): boolean {
		return this.#isValid;
	}

	addValidator(validator: UmbValidator): void {
		if (this.#validators.includes(validator)) return;
		this.#validators.push(validator);
		//validator.addEventListener('change', this.#onValidatorChange);
		if (this.#validationMode) {
			this.validate();
		}
	}
	removeValidator(validator: UmbValidator): void {
		const index = this.#validators.indexOf(validator);
		if (index !== -1) {
			// Remove the validator:
			this.#validators.splice(index, 1);
			// If we are in validation mode then we should re-validate to focus next invalid element:
			if (this.#validationMode) {
				this.validate();
			}
		}
	}

	/*#onValidatorChange = (e: Event) => {
		const target = e.target as unknown as UmbValidator | undefined;
		if (!target) {
			console.error('Validator did not exist.');
			return;
		}
		const dataPath = target.dataPath;
		if (!dataPath) {
			console.error('Validator did not exist or did not provide a data-path.');
			return;
		}

		if (target.isValid) {
			this.messages.removeMessagesByTypeAndPath('client', dataPath);
		} else {
			this.messages.addMessages('client', dataPath, target.getMessages());
		}
	};*/

	/**
	 *
	 * @returns succeed {Promise<boolean>} - Returns a promise that resolves to true if the validator succeeded, this depends on the validators and wether forceSucceed is set.
	 */
	async validate(): Promise<void> {
		// TODO: clear server messages here?, well maybe only if we know we will get new server messages? Do the server messages hook into the system like another validator?
		this.#validationMode = true;

		const resultsStatus = await Promise.all(this.#validators.map((v) => v.validate())).then(
			() => Promise.resolve(true),
			() => Promise.resolve(false),
		);

		// If we have any messages then we are not valid, otherwise lets check the validation results: [NL]
		// This enables us to keep client validations though UI is not present anymore — because the client validations got defined as messages. [NL]
		const isValid = this.messages.getHasAnyMessages() ? false : resultsStatus;

		this.#isValid = isValid;

		if (isValid === false) {
			// Focus first invalid element:
			this.focusFirstInvalidElement();
			return Promise.reject();
		}

		return Promise.resolve();
	}

	focusFirstInvalidElement(): void {
		const firstInvalid = this.#validators.find((v) => !v.isValid);
		if (firstInvalid) {
			firstInvalid.focusFirstInvalidElement();
		}
	}

	reset(): void {
		this.#validationMode = false;
		this.#validators.forEach((v) => v.reset());
	}

	#destroyValidators(): void {
		if (this.#validators === undefined || this.#validators.length === 0) return;
		this.#validators.forEach((validator) => {
			validator.destroy();
			//validator.removeEventListener('change', this.#runValidate);
		});
		this.#validators = [];
	}

	destroy(): void {
		this.#destroyValidators();
		super.destroy();
	}
}
