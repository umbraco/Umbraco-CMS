import type { UmbValidationMessageTranslator } from '../interfaces/validation-message-translator.interface.js';
import type { UmbValidator } from '../interfaces/validator.interface.js';
import { UMB_VALIDATION_CONTEXT } from './validation.context-token.js';
import { UMB_SERVER_MODEL_VALIDATION_CONTEXT } from './server-model-validation.context-token.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbServerModelValidationContext
	extends UmbContextBase<UmbServerModelValidationContext>
	implements UmbValidator
{
	#validatePromise?: Promise<boolean>;
	#validatePromiseResolve?: (valid: boolean) => void;

	#context?: typeof UMB_VALIDATION_CONTEXT.TYPE;
	#isValid = true;

	#translators: Array<UmbValidationMessageTranslator> = [];

	// Hold server feedback...
	#serverFeedback: Record<string, Array<string>> = {};

	constructor(host: UmbControllerHost) {
		super(host, UMB_SERVER_MODEL_VALIDATION_CONTEXT);
		this.consumeContext(UMB_VALIDATION_CONTEXT, (context) => {
			if (this.#context) {
				this.#context.removeValidator(this);
			}
			this.#context = context;
			context.addValidator(this);

			// Run translators?
		});
	}

	async askServerForValidation(requestPromise: Promise<{ data: string | undefined; error: any }>): Promise<void> {
		this.#context?.messages.removeMessagesByType('server');

		this.#serverFeedback = {};
		this.#isValid = false;
		//this.#validatePromiseReject?.();
		this.#validatePromise = new Promise<boolean>((resolve) => {
			this.#validatePromiseResolve = resolve;
		});
		// Ask the server for validation...
		const { data, error } = await requestPromise;

		console.log('VALIDATE — Got server response:');
		console.log(data, error);

		this.#isValid = false;
		this.#validatePromiseResolve?.(false);
		this.#validatePromiseResolve = undefined;
		//this.#validatePromise = undefined;
	}

	addTranslator(translator: UmbValidationMessageTranslator): void {
		if (this.#translators.indexOf(translator) === -1) {
			this.#translators.push(translator);
		}
	}

	removeTranslator(translator: UmbValidationMessageTranslator): void {
		const index = this.#translators.indexOf(translator);
		if (index !== -1) {
			this.#translators.splice(index, 1);
		}
	}

	get isValid(): boolean {
		return this.#isValid;
	}
	async validate(): Promise<void> {
		if (this.#validatePromise) {
			await this.#validatePromise;
		}
		return this.#isValid ? Promise.resolve() : Promise.reject();
	}

	reset(): void {}

	focusFirstInvalidElement(): void {}

	hostConnected(): void {
		super.hostConnected();
		if (this.#context) {
			this.#context.addValidator(this);
		}
	}
	hostDisconnected(): void {
		super.hostDisconnected();
		if (this.#context) {
			this.#context.removeValidator(this);
			this.#context = undefined;
		}
	}

	destroy(): void {
		// TODO: make sure we destroy things properly:
		this.#translators = [];
		super.destroy();
	}
}
