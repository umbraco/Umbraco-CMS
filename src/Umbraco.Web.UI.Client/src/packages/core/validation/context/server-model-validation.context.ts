import type { UmbValidationMessageTranslator } from '../interfaces/validation-message-translator.interface.js';
import type { UmbValidator } from '../interfaces/validator.interface.js';
import { UMB_VALIDATION_CONTEXT } from './validation.context-token.js';
import { UMB_SERVER_MODEL_VALIDATION_CONTEXT } from './server-model-validation.context-token.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

export class UmbServerModelValidationContext
	extends UmbContextBase<UmbServerModelValidationContext>
	implements UmbValidator
{
	#validatePromise?: Promise<boolean>;
	#validatePromiseResolve?: (valid: boolean) => void;
	#validatePromiseReject?: () => void;

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

	async askServerForValidation(requestPromise: Promise<{ data: string | undefined }>): Promise<void> {
		this.#context?.messages.removeMessagesByType('server');

		this.#validatePromiseReject?.();
		this.#validatePromise = new Promise<boolean>((resolve, reject) => {
			this.#validatePromiseResolve = resolve;
			this.#validatePromiseReject = reject;
		});
		this.#serverFeedback = {};
		// Ask the server for validation...
		const { data } = await tryExecuteAndNotify(this, requestPromise);

		console.log('VALIDATE — Got server response:');
		console.log(data);

		this.#validatePromiseResolve?.(true);
		this.#validatePromiseResolve = undefined;
		this.#validatePromiseReject = undefined;
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
	validate(): Promise<boolean> {
		// TODO: Return to this decision once we have a bit more implementation to perspectives against: [NL]
		// If we dont have a validatePromise, we valid cause then no one has called askServerForValidation(). [NL] (I might change my mind about this one, to then say we are invalid unless we have been validated by the server... )
		return this.#validatePromise ?? Promise.resolve(true);
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
