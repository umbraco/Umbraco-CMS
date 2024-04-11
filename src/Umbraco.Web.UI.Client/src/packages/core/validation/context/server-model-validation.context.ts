import type { UmbValidationMessageTranslator } from '../translators/validation-message-translator.interface.js';
import type { UmbValidator } from '../interfaces/validator.interface.js';
import { UMB_VALIDATION_CONTEXT } from './validation.context-token.js';
import { UMB_SERVER_MODEL_VALIDATION_CONTEXT } from './server-model-validation.context-token.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { ApiError, CancelError } from '@umbraco-cms/backoffice/external/backend-api';

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

	async askServerForValidation(
		data: unknown,
		requestPromise: Promise<{ data: unknown; error: ApiError | CancelError | undefined }>,
	): Promise<void> {
		this.#context?.messages.removeMessagesByType('server');

		this.#serverFeedback = [];
		this.#isValid = false;
		//this.#validatePromiseReject?.();
		this.#validatePromise = new Promise<void>((resolve) => {
			this.#validatePromiseResolve = resolve;
		});

		// Ask the server for validation...
		//const { data: feedback, error } = await requestPromise;
		await requestPromise;

		//console.log('VALIDATE — Got server response:');
		//console.log(data, error);

		// Store this state of the data for translator look ups:
		this.#data = data;
		/*
		const fixedData = {
			type: 'Error',
			title: 'Validation failed',
			status: 400,
			detail: 'One or more properties did not pass validation',
			operationStatus: 'PropertyValidationError',
			errors: {
				'$.values[0].value': ['#validation.invalidPattern'],
			} as Record<string, Array<string>>,
			missingProperties: [],
		};

		Object.keys(fixedData.errors).forEach((path) => {
			this.#serverFeedback.push({ path, messages: fixedData.errors[path] });
		});*/

		//this.#isValid = data ? true : false;
		//this.#isValid = false;
		this.#isValid = true;
		this.#validatePromiseResolve?.();
		this.#validatePromiseResolve = undefined;
		//this.#validatePromise = undefined;

		this.#serverFeedback = this.#serverFeedback.flatMap(this.#executeTranslatorsOnFeedback);
	}

	#executeTranslatorsOnFeedback = (feedback: ServerFeedbackEntry) => {
		return this.#translators.flatMap((translator) => {
			if (translator.match(feedback.path)) {
				const newPath = translator.translate(feedback.path);

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
