import type { UmbValidator } from '../interfaces/validator.interface.js';
import type { UmbValidationMessageTranslator } from '../translators/index.js';
import { GetValueByJsonPath } from '../utils/json-path.function.js';
import { UMB_VALIDATION_CONTEXT } from '../context/validation.context-token.js';
import { type UmbValidationMessage, UmbValidationMessagesManager } from '../context/validation-messages.manager.js';
import type { UmbContextProviderController } from '@umbraco-cms/backoffice/context-api';
import { type UmbClassInterface, UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';

/**
 * Helper method to replace the start of a string with another string.
 * @param path {string}
 * @param startFrom {string}
 * @param startTo {string}
 * @returns {string}
 */
function ReplaceStartOfString(path: string, startFrom: string, startTo: string): string {
	if (path.startsWith(startFrom + '.')) {
		return startTo + path.slice(startFrom.length);
	}
	return path;
}

/**
 * Validation Context is the core of Validation.
 * It hosts Validators that has to validate for the context to be valid.
 * It can also be used as a Validator as part of a parent Validation Context.
 */
export class UmbValidationController extends UmbControllerBase implements UmbValidator {
	// The current provider controller, that is providing this context:
	#providerCtrl?: UmbContextProviderController<
		UmbValidationController,
		UmbValidationController,
		UmbValidationController
	>;

	// Local version of the data send to the server, only use-case is for translation.
	#translationData = new UmbObjectState<any>(undefined);
	translationDataOf(path: string): any {
		return this.#translationData.asObservablePart((data) => GetValueByJsonPath(data, path));
	}
	setTranslationData(data: any): void {
		this.#translationData.setValue(data);
	}
	getTranslationData(): any {
		return this.#translationData.getValue();
	}

	#validators: Array<UmbValidator> = [];
	#validationMode: boolean = false;
	#isValid: boolean = false;

	#parent?: UmbValidationController;
	#parentMessages?: Array<UmbValidationMessage>;
	#localMessages?: Array<UmbValidationMessage>;
	#baseDataPath?: string;

	public readonly messages = new UmbValidationMessagesManager();

	constructor(host: UmbControllerHost) {
		// This is overridden to avoid setting a controllerAlias, this might make sense, but currently i want to leave it out. [NL]
		super(host);
	}

	/**
	 * Add a path translator to this validation context.
	 * @param translator
	 */
	async addTranslator(translator: UmbValidationMessageTranslator) {
		this.messages.addTranslator(translator);
	}

	/**
	 * Remove a path translator from this validation context.
	 * @param translator
	 */
	async removeTranslator(translator: UmbValidationMessageTranslator) {
		this.messages.removeTranslator(translator);
	}

	#currentProvideHost?: UmbClassInterface;
	/**
	 * Provide this validation context to a specific controller host.
	 * This can be used to Host a validation context in a Workspace, but provide it on a certain scope, like a specific Workspace View.
	 * @param controllerHost {UmbClassInterface}
	 */
	provideAt(controllerHost: UmbClassInterface): void {
		if (this.#currentProvideHost === controllerHost) return;
		this.#providerCtrl?.destroy();
		this.#currentProvideHost = controllerHost;
		this.#providerCtrl = controllerHost.provideContext(UMB_VALIDATION_CONTEXT, this);
	}

	/**
	 * Define a specific data path for this validation context.
	 * This will turn this validation context into a sub-context of the parent validation context.
	 * This means that a two-way binding for messages will be established between the parent and the sub-context.
	 * And it will inherit the Translation Data from its parent.
	 *
	 * messages and data will be localizes accordingly to the given data path.
	 * @param dataPath {string} - The data path to bind this validation context to.
	 * @example
	 * ```ts
	 * const validationContext = new UmbValidationContext(this);
	 * validationContext.setDataPath("$.values[?(@.alias == 'my-property')].value");
	 * ```
	 *
	 * A message with the path: '$.values[?(@.alias == 'my-property')].value.innerProperty', will for above example become '$.innerProperty' for the local Validation Context.
	 */
	setDataPath(dataPath: string): void {
		if (this.#baseDataPath) {
			if (this.#baseDataPath === dataPath) return;
			// Just fire an error, as I haven't made the right clean up jet. Or haven't thought about what should happen if it changes while already setup.
			// cause maybe all the messages should be removed as we are not interested in the old once any more. But then on the other side, some might be relevant as this is the same entity that changed its paths?
			throw new Error('Data path is already set, we do not support changing the context data-path as of now.');
		}
		if (!dataPath) return;
		this.#baseDataPath = dataPath;

		this.consumeContext(UMB_VALIDATION_CONTEXT, (parent) => {
			if (this.#parent) {
				this.#parent.removeValidator(this);
			}
			this.#parent = parent;
			parent.addValidator(this);

			this.messages.clear();

			this.observe(parent.translationDataOf(dataPath), (data) => {
				this.setTranslationData(data);
			});

			this.observe(
				parent.messages.messagesOfPathAndDescendant(dataPath),
				(msgs) => {
					//this.messages.appendMessages(msgs);
					if (this.#parentMessages) {
						// Remove the local messages that does not exist in the parent anymore:
						const toRemove = this.#parentMessages.filter((msg) => !msgs.find((m) => m.key === msg.key));
						toRemove.forEach((msg) => {
							this.messages.removeMessageByKey(msg.key);
						});
					}
					this.#parentMessages = msgs;
					msgs.forEach((msg) => {
						const path = ReplaceStartOfString(msg.path, this.#baseDataPath!, '$');
						// Notice, the local message uses the same key. [NL]
						this.messages.addMessage(msg.type, path, msg.body, msg.key);
					});
				},
				'observeParentMessages',
			);

			this.observe(
				this.messages.messages,
				(msgs) => {
					if (!this.#parent) return;
					//this.messages.appendMessages(msgs);
					if (this.#localMessages) {
						// Remove the parent messages that does not exist locally anymore:
						const toRemove = this.#localMessages.filter((msg) => !msgs.find((m) => m.key === msg.key));
						toRemove.forEach((msg) => {
							this.#parent!.messages.removeMessageByKey(msg.key);
						});
					}
					this.#localMessages = msgs;
					msgs.forEach((msg) => {
						// replace this.#baseDataPath (if it starts with it) with $ in the path, so it becomes relative to the parent context
						const path = ReplaceStartOfString(msg.path, '$', this.#baseDataPath!);
						// Notice, the parent message uses the same key. [NL]
						this.#parent!.messages.addMessage(msg.type, path, msg.body, msg.key);
					});
				},
				'observeLocalMessages',
			);
		}).skipHost();
		// Notice skipHost ^^, this is because we do not want it to consume it self, as this would be a match for this consumption, instead we will look at the parent and above. [NL]
	}

	/**
	 * Get if this context is valid.
	 * Notice this does not verify the validity.
	 * @returns {boolean}
	 */
	get isValid(): boolean {
		return this.#isValid;
	}

	/**
	 * Add a validator to this context.
	 * This validator will have to be valid for the context to be valid.
	 * If the context is in validation mode, the validator will be validated immediately.
	 * @param validator { UmbValidator } - The validator to add to this context.
	 */
	addValidator(validator: UmbValidator): void {
		if (this.#validators.includes(validator)) return;
		this.#validators.push(validator);
		//validator.addEventListener('change', this.#onValidatorChange);
		if (this.#validationMode) {
			this.validate();
		}
	}

	/**
	 * Remove a validator from this context.
	 * @param validator {UmbValidator} - The validator to remove from this context.
	 */
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
	 * Validate this context, all the validators of this context will be validated.
	 * Notice its a recursive check meaning sub validation contexts also validates their validators.
	 * @returns succeed {Promise<boolean>} - Returns a promise that resolves to true if the validation succeeded.
	 */
	async validate(): Promise<void> {
		// TODO: clear server messages here?, well maybe only if we know we will get new server messages? Do the server messages hook into the system like another validator?
		this.#validationMode = true;

		const resultsStatus = await Promise.all(this.#validators.map((v) => v.validate())).then(
			() => true,
			() => false,
		);

		if (!this.messages) {
			// This Context has been destroyed while is was validating, so we should not continue.
			return;
		}

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

	/**
	 * Focus the first invalid element that this context can find.
	 */
	focusFirstInvalidElement(): void {
		const firstInvalid = this.#validators.find((v) => !v.isValid);
		if (firstInvalid) {
			firstInvalid.focusFirstInvalidElement();
		}
	}

	/**
	 * Reset the validation state of this context.
	 */
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
		this.#providerCtrl = undefined;
		if (this.#parent) {
			this.#parent.removeValidator(this);
		}
		this.#parent = undefined;
		this.#destroyValidators();
		this.messages?.destroy();
		(this.messages as unknown) = undefined;
		super.destroy();
	}
}
