import type { UmbValidator } from '../interfaces/validator.interface.js';
import type { UmbValidationPathTranslator } from '../types.js';
import { UmbValidationPathTranslationController } from '../controllers/validation-path-translation/index.js';
import { UMB_VALIDATION_CONTEXT } from './validation.context-token.js';
import { UMB_SERVER_MODEL_VALIDATOR_CONTEXT } from './server-model-validator.context-token.js';
import type { UmbValidationMessage } from './validation-messages.manager.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';
import type { ClassConstructor } from '@umbraco-cms/backoffice/extension-api';
import { UmbId } from '@umbraco-cms/backoffice/id';
import type { UmbApiError } from '@umbraco-cms/backoffice/resources';

export class UmbServerModelValidatorContext extends UmbContextBase implements UmbValidator {
	#pathTranslators: Array<ClassConstructor<UmbValidationPathTranslator<any>>> = [];

	#validatePromise?: Promise<void>;
	#validatePromiseResolve?: () => void;
	#validatePromiseReject?: () => void;

	#context?: typeof UMB_VALIDATION_CONTEXT.TYPE;
	#isValid = true;

	#data: any;
	getData(): any {
		return this.#data;
	}

	constructor(host: UmbControllerHost) {
		super(host, UMB_SERVER_MODEL_VALIDATOR_CONTEXT);
		this.consumeContext(UMB_VALIDATION_CONTEXT, (context) => {
			if (this.#context) {
				this.#context.removeValidator(this);
			}
			this.#context = context;
			context?.addValidator(this);

			// Run translators?
		}).asPromise({ preventTimeout: true });
	}

	addPathTranslator(translator: ClassConstructor<UmbValidationPathTranslator<any>>): void {
		this.#pathTranslators.push(translator);
	}

	async askServerForValidation(data: unknown, requestPromise: Promise<UmbDataSourceResponse<string>>): Promise<void> {
		this.#context?.messages.removeMessagesByType('server');

		this.#isValid = false;
		this.#validatePromiseReject?.();
		this.#validatePromise = new Promise<void>((resolve, reject) => {
			this.#validatePromiseResolve = resolve;
			this.#validatePromiseReject = reject;
		});

		// Store this state of the data for translator look ups:
		this.#data = data;
		// Ask the server for validation...
		const { error } = await requestPromise;

		if (this.#data !== data) {
			console.warn(
				'Data has changed since validation request was sent to server. This validation request will be rejected.',
			);
			return Promise.reject();
		}

		this.#isValid = error ? false : true;
		if (this.#isValid) {
			// Send data to context for translation:
			this.#context?.setTranslationData(undefined);
		} else {
			if (!this.#context) {
				throw new Error('No context available for translation.');
			}
			// Send data to context for translation:
			this.#context.setTranslationData(data);

			let messages: Array<UmbValidationMessage> = [];

			// We are missing some typing here, but we will just go wild with 'as any': [NL]
			const errorBody = (error as UmbApiError).problemDetails;
			// Check if there are validation errors, since the error might be a generic ApiError
			if (errorBody?.errors) {
				Object.keys(errorBody.errors).forEach((path) => {
					const newBodies = errorBody.errors![path];
					// Correct path to ensure it starts with `$.` (notice it mainly starts with `$.`, but the server sometimes does not include it)
					if (path.startsWith('$.')) {
						// Everything is good.
					} else {
						if (path.startsWith('.')) {
							path = '$' + path;
						} else {
							path = '$.' + path;
						}
					}
					newBodies.forEach((body: string) => messages.push({ type: 'server', key: UmbId.new(), path, body }));
					//this.#context!.messages.addMessages('server', path, errorBody.errors[path]);
				});
			}

			if (messages.length > 0) {
				const ctrl = new UmbValidationPathTranslationController(this, {
					translationData: this.#data,
					pathTranslators: this.#pathTranslators,
				});

				messages = await ctrl.translate(messages);
				this.#context.messages.addMessageObjects(messages);
			}
		}

		this.#validatePromiseResolve?.();
		this.#validatePromiseResolve = undefined;
		this.#validatePromiseReject = undefined;
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

	reset(): void {
		this.#isValid = true;
		this.#validatePromiseReject?.();
		this.#validatePromiseResolve = undefined;
		this.#validatePromiseReject = undefined;
	}

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
		super.destroy();
	}
}
