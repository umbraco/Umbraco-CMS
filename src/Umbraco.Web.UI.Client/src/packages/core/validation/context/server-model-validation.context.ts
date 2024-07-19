import type { UmbValidationMessageTranslator } from '../translators/validation-message-translator.interface.js';
import type { UmbValidator } from '../interfaces/validator.interface.js';
import { UMB_VALIDATION_CONTEXT } from './validation.context-token.js';
import { UMB_SERVER_MODEL_VALIDATION_CONTEXT } from './server-model-validation.context-token.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

type ServerFeedbackEntry = { path: string; messages: Array<string> };

export class UmbServerModelValidationContext
	extends UmbContextBase<UmbServerModelValidationContext>
	implements UmbValidator
{
	#validatePromise?: Promise<void>;
	#validatePromiseResolve?: () => void;

	#context?: typeof UMB_VALIDATION_CONTEXT.TYPE;
	#isValid = true;

	#data: any;
	getData(): any {
		return this.#data;
	}
	#translators: Array<UmbValidationMessageTranslator> = [];

	// Hold server feedback...
	#serverFeedback: Array<ServerFeedbackEntry> = [];

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

	async askServerForValidation(data: unknown, requestPromise: Promise<UmbDataSourceResponse<string>>): Promise<void> {
		this.#context?.messages.removeMessagesByType('server');

		this.#serverFeedback = [];
		this.#isValid = false;
		//this.#validatePromiseReject?.();
		this.#validatePromise = new Promise<void>((resolve) => {
			this.#validatePromiseResolve = resolve;
		});

		// Store this state of the data for translator look ups:
		this.#data = data;
		// Ask the server for validation...
		const { error } = await requestPromise;

		this.#isValid = error ? false : true;

		if (!this.#isValid) {
			// We are missing some typing here, but we will just go wild with 'as any': [NL]
			const readErrorBody = (error as any).body;
			// Check if there are validation errors, since the error might be a generic ApiError
			if (readErrorBody?.errors) {
				Object.keys(readErrorBody.errors).forEach((path) => {
					this.#serverFeedback.push({ path, messages: readErrorBody.errors[path] });
				});
			}
		}

		this.#validatePromiseResolve?.();
		this.#validatePromiseResolve = undefined;

		// Translate feedback:
		this.#serverFeedback = this.#serverFeedback.flatMap(this.#executeTranslatorsOnFeedback);
	}

	#executeTranslatorsOnFeedback = (feedback: ServerFeedbackEntry) => {
		return this.#translators.flatMap((translator) => {
			let newPath: string | undefined;
			if ((newPath = translator.translate(feedback.path))) {
				// TODO: I might need to restructure this part for adjusting existing feedback with a part-translation.
				// Detect if some part is unhandled?
				// If so only make a partial translation on the feedback, add a message for the handled part.
				// then return [ of the partial translated feedback, and the partial handled part. ];

				//  TODO:Check if there was any temporary messages base on this path, like if it was partial-translated at one point..

				this.#context?.messages.addMessages('server', newPath, feedback.messages);
				// by not returning anything this feedback gets removed from server feedback..
				return [];
			}
			return feedback;
		});
	};

	addTranslator(translator: UmbValidationMessageTranslator): void {
		if (this.#translators.indexOf(translator) === -1) {
			this.#translators.push(translator);
		}
		// execute translators here?
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

	override hostConnected(): void {
		super.hostConnected();
		if (this.#context) {
			this.#context.addValidator(this);
		}
	}
	override hostDisconnected(): void {
		super.hostDisconnected();
		if (this.#context) {
			this.#context.removeValidator(this);
			this.#context = undefined;
		}
	}

	override destroy(): void {
		// TODO: make sure we destroy things properly:
		this.#translators = [];
		super.destroy();
	}
}
