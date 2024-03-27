import type { UmbValidator } from '../interfaces/validator.interface.js';
import { UMB_VALIDATION_CONTEXT } from './validation.context-token.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbValidationContext extends UmbContextBase<UmbValidationContext> implements UmbValidator {
	#validators: Array<UmbValidator> = [];
	#validationMode: boolean = false;
	#isValid: boolean = false;
	#preventFail: boolean = false;

	constructor(host: UmbControllerHost) {
		super(host, UMB_VALIDATION_CONTEXT);
	}

	get isValid(): boolean {
		return this.#isValid;
	}

	preventFail(): void {
		this.#preventFail = true;
	}

	allowFail(): void {
		this.#preventFail = false;
	}

	addValidator(validator: UmbValidator) {
		this.#validators.push(validator);
		//validator.addEventListener('change', this.#runValidate);
	}
	removeValidator(validator: UmbValidator) {
		const index = this.#validators.indexOf(validator);
		if (index !== -1) {
			this.#validators.splice(index, 1);
			//validator.removeEventListener('change', this.#runValidate);
		}
	}

	#runValidate = this.validate.bind(this);

	/**
	 *
	 * @returns succeed {Promise<boolean>} - Returns a promise that resolves to true if the validator succeeded, this depends on the validators and wether forceSucceed is set.
	 */
	async validate(): Promise<boolean> {
		this.#validationMode = true;
		const results = await Promise.all(this.#validators.map((v) => v.validate()));
		const isValid = results.every((r) => r);
		this.#isValid = isValid;

		// Focus first invalid element:
		if (!isValid) {
			const firstInvalid = this.#validators.find((v) => !v.isValid);
			if (firstInvalid) {
				firstInvalid.focusFirstInvalidElement();
			}
		}

		return this.#preventFail ? true : isValid;
	}

	focusFirstInvalidElement(): void {
		const firstInvalid = this.#validators.find((v) => !v.isValid);
		if (firstInvalid) {
			firstInvalid.focusFirstInvalidElement();
		}
	}

	getMessages(): string[] {
		return this.#validators.reduce((acc, v) => acc.concat(v.getMessages()), [] as string[]);
	}

	reset(): void {
		this.#validationMode = false;
		this.#validators.forEach((v) => v.reset());
	}

	#destroyValidators() {
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
