import type { UmbValidator } from '../interfaces/validator.interface.js';
import { UmbValidationMessagesManager } from './validation-messages.manager.js';
import { UMB_VALIDATION_CONTEXT } from './validation.context-token.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbValidationContext extends UmbContextBase<UmbValidationContext> implements UmbValidator {
	#validators: Array<UmbValidator> = [];
	#validationMode: boolean = false;
	#isValid: boolean = false;

	#parent?: UmbValidationContext;
	#baseDataPath?: string;

	public readonly messages = new UmbValidationMessagesManager();

	constructor(host: UmbControllerHost) {
		super(host, UMB_VALIDATION_CONTEXT);
	}

	setDataPath(dataPath: string): void {
		if (this.#baseDataPath) {
			// Just fire an error, as I haven't made the right clean up jet. Or haven't thought about what should happen if it changes while already setup.
			// cause maybe all the messages should be removed as we are not interested in the old once any more. But then on the other side, some might be relevant as this is the same entity that changed its paths?
			throw new Error('Data path is already set, we do not support changing the context data-path as of now.');
		}
		this.#baseDataPath = dataPath;

		this.consumeContext(UMB_VALIDATION_CONTEXT, (parent) => {
			if (this.#parent) {
				this.#parent.removeValidator(this);
			}
			this.#parent = parent;
			parent.addValidator(this);

			// observe parent messages that fits with the path?.
			// — Transfer message, without the base path?

			// observe our messages and make sure to propagate them? or observe our messages, if we have one, set one message to the parent with out base path. Or parse all?.
			// Maybe it has to actively be add or remove...

			// Question is do we want sub messages to be added to the parent? or do we want to keep them separate?.
			// How about removing sub messages, a sub message could come from the server/parent. so who decides to remove it? I would say it still should be the property editor.
			// So maybe only remove if they exist upward, but not append upward? Maybe only append upward one for the basePath, and then if we have one message when getting spun up, we can run validation and if validation is good we remove the base-path from the parent.

			// Whats the value of having them local?
			// It makes it more complex,
			// It makes it harder to debug
			// But it makes it easier to remove translators as they get taken out. But maybe this is handled by the controller pattern?
			// The amount of translator instances will still be the same, as we then end up with multiple validation contexts and therefor just more things, not necessary less.

			// We may still do so, but only for Workspaces.
		}).skipHost();
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

	override destroy(): void {
		if (this.#parent) {
			this.#parent.removeValidator(this);
		}
		this.#parent = undefined;
		this.#destroyValidators();
		this.messages?.destroy();
		(this.messages as any) = undefined;
		super.destroy();
	}
}
