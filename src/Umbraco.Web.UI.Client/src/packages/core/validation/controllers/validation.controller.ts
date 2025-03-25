import type { UmbValidator } from '../interfaces/validator.interface.js';
import type { UmbValidationMessageTranslator } from '../translators/index.js';
import { GetValueByJsonPath } from '../utils/json-path.function.js';
import { UMB_VALIDATION_CONTEXT } from '../context/validation.context-token.js';
import { type UmbValidationMessage, UmbValidationMessagesManager } from '../context/validation-messages.manager.js';
import type { UmbContextProviderController } from '@umbraco-cms/backoffice/context-api';
import { type UmbClassInterface, UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { ReplaceStartOfPath } from '../utils/replace-start-of-path.function.js';
import type { UmbVariantId } from '../../variant/variant-id.class.js';
import { UmbDeprecation } from '../../utils/deprecation/deprecation.js';

const Regex = /@\.culture == ('[^']*'|null) *&& *@\.segment == ('[^']*'|null)/g;

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
	#inUnprovidingState: boolean = false;

	// @reprecated - Will be removed in v.17
	// Local version of the data send to the server, only use-case is for translation.
	#translationData = new UmbObjectState<any>(undefined);
	/**
	 * @deprecated Use extension type 'propertyValidationPathTranslator' instead. Will be removed in v.17
	 */
	translationDataOf(path: string): any {
		return this.#translationData.asObservablePart((data) => GetValueByJsonPath(data, path));
	}
	/**
	 * @deprecated Use extension type 'propertyValidationPathTranslator' instead. Will be removed in v.17
	 */
	setTranslationData(data: any): void {
		this.#translationData.setValue(data);
	}
	/**
	 * @deprecated Use extension type 'propertyValidationPathTranslator' instead. Will be removed in v.17
	 */
	getTranslationData(): any {
		new UmbDeprecation({
			removeInVersion: '17',
			deprecated: 'getTranslationData',
			solution: 'getTranslationData is deprecated.',
		}).warn();

		return this.#translationData.getValue();
	}

	#validators: Array<UmbValidator> = [];
	#validationMode: boolean = false;
	#isValid: boolean = false;

	#parent?: UmbValidationController;
	#sync?: boolean;
	#parentMessages?: Array<UmbValidationMessage>;
	#localMessages?: Array<UmbValidationMessage>;
	#baseDataPath?: string;

	#variantId?: UmbVariantId;

	public readonly messages = new UmbValidationMessagesManager();

	constructor(host: UmbControllerHost) {
		// This is overridden to avoid setting a controllerAlias, this might make sense, but currently i want to leave it out. [NL]
		super(host);
	}

	setVariantId(variantId: UmbVariantId): void {
		this.#variantId = variantId;
		// @.culture == null && @.segment == null
		this.messages.filter((msg) => {
			// Figure out how many times '@.culture ==' is present in the path:
			//const cultureMatches = (msg.path.match(/@\.culture ==/g) || []);
			// I like a Regex that finds all the @.culture == and @.segment == in the path. they are adjacent. and I like to know the value following '== '
			const variantMatches = [...msg.path.matchAll(Regex)];

			// if not cultures, then we like to keep it:
			if (variantMatches.length === 0) return true;

			return (
				// Find any bad matches:
				!variantMatches.some((match) => {
					const culture = match[1] === 'null' ? null : match[1].substring(1, match[1].length - 1);
					const segment = match[2] === 'null' ? null : match[2].substring(1, match[2].length - 1);

					const result =
						(culture !== null && culture !== this.#variantId!.culture) || segment !== this.#variantId!.segment;
					return result;
				})
			);
		});
	}

	getVariantId(): UmbVariantId | undefined {
		return this.#variantId;
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
		// Because this may have been destroyed at this point. and because we do not know if a context has been destroyed, then we allow this call, but let it soft-fail if messages does not exists. [NL]
		this.messages?.removeTranslator(translator);
	}

	#currentProvideHost?: UmbClassInterface;
	/**
	 * Provide this validation context to a specific controller host.
	 * This can be used to Host a validation context in a Workspace, but provide it on a certain scope, like a specific Workspace View.
	 * @param controllerHost {UmbClassInterface}
	 */
	provideAt(controllerHost: UmbClassInterface): void {
		if (this.#currentProvideHost === controllerHost) return;

		this.unprovide();

		if (this.messages === undefined) {
			throw new Error('This Validation Context has been destroyed and can not be provided.');
		}
		this.#currentProvideHost = controllerHost;
		this.#providerCtrl = controllerHost.provideContext(UMB_VALIDATION_CONTEXT, this);
	}

	unprovide(): void {
		if (this.#providerCtrl) {
			// We need to set this in Unprovide state, so this context can be provided again later.
			this.#inUnprovidingState = true;
			this.#providerCtrl.destroy();
			this.#providerCtrl = undefined;
			this.#inUnprovidingState = false;
			this.#currentProvideHost = undefined;
		}
	}

	/**
	 * Define a specific data path for this validation context.
	 * This will turn this validation context into a sub-context of the parent validation context.
	 * This will make this context inherit the messages from the parent validation context.
	 * @see {@link report} Call `report()` to propagate changes to the parent context.
	 * @see {@link autoReport} Call `autoReport()` to continuously synchronize changes to the parent context.
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
		if (!dataPath) {
			this.#stopInheritance();
			return;
		}

		this.consumeContext(UMB_VALIDATION_CONTEXT, (parent) => {
			this.inheritFrom(parent, dataPath);
		}).skipHost();
		// Notice skipHost ^^, this is because we do not want it to consume it self, as this would be a match for this consumption, instead we will look at the parent and above. [NL]
	}

	/**
	 * Inherit from a parent validation context, notice you should only use this method if you have the validation context at hand. Otherwise use setDataPath which uses Context-API to retrieve the right parent.
	 * @param {UmbValidationController} parent - The parent validation context to inherit from.
	 * @param {string} dataPath - The data path to bind this validation context to.
	 */
	inheritFrom(parent: UmbValidationController, dataPath: string): void {
		if (this.#parent) {
			this.#parent.removeValidator(this);
		}
		this.#parent = parent;
		this.#readyToSync();

		this.messages.clear();
		this.#localMessages = undefined;

		this.#baseDataPath = dataPath;

		// @deprecated - Will be removed in v.17
		this.observe(
			parent.translationDataOf(dataPath),
			(data) => {
				this.setTranslationData(data);
			},
			'observeTranslationData',
		);

		this.observe(
			parent.messages.messagesOfPathAndDescendant(dataPath),
			(msgs) => {
				this.messages.initiateChange();
				if (this.#parentMessages) {
					// Remove the local messages that does not exist in the parent anymore:
					const toRemove = this.#parentMessages.filter((msg) => !msgs.find((m) => m.key === msg.key));
					this.messages.removeMessageByKeys(toRemove.map((msg) => msg.key));
				}
				this.#parentMessages = msgs;
				if (this.#baseDataPath === '$') {
					this.messages.addMessageObjects(msgs);
				} else {
					msgs.forEach((msg) => {
						const path = ReplaceStartOfPath(msg.path, this.#baseDataPath!, '$');
						if (path === undefined) {
							throw new Error(
								'Path was not transformed correctly and can therefor not be transfered to the local validation context messages.',
							);
						}
						// Notice, the local message uses the same key. [NL]
						this.messages.addMessage(msg.type, path, msg.body, msg.key);
					});
				}

				this.#localMessages = this.messages.getNotFilteredMessages();
				this.messages.finishChange();
			},
			'observeParentMessages',
		);
	}

	#stopInheritance(): void {
		this.removeUmbControllerByAlias('observeTranslationData');
		this.removeUmbControllerByAlias('observeParentMessages');

		if (this.#parent) {
			this.#parent.removeValidator(this);
		}
		this.messages.clear();
		this.#localMessages = undefined;
		this.setTranslationData(undefined);
	}

	#readyToSync() {
		if (this.#sync && this.#parent) {
			this.#parent.addValidator(this);
		}
	}

	/**
	 * Continuously synchronize the messages from this context to the parent context.
	 */
	autoReport() {
		this.#sync = true;
		this.#readyToSync();
		this.observe(this.messages.messages, this.#transferMessages, 'observeLocalMessages');
	}

	// no need for this method at this movement. [NL]
	/*
	#stopSync() {
		this.removeUmbControllerByAlias('observeLocalMessages');
	}
	*/

	/**
	 * Perform a one time transfer of the messages from this context to the parent context.
	 */
	report(): void {
		if (!this.#parent) return;

		if (!this.#sync) {
			this.#transferMessages(this.messages.getNotFilteredMessages());
		}
	}

	#transferMessages = (msgs: Array<UmbValidationMessage>) => {
		if (!this.#parent) return;

		this.#parent!.messages.initiateChange();

		if (this.#localMessages) {
			// Remove the parent messages that does not exist locally anymore:
			const toRemove = this.#localMessages.filter((msg) => !msgs.find((m) => m.key === msg.key));
			this.#parent!.messages.removeMessageByKeys(toRemove.map((msg) => msg.key));
		}

		if (this.#baseDataPath === '$') {
			this.#parent!.messages.addMessageObjects(msgs);
		} else {
			msgs.forEach((msg) => {
				// replace this.#baseDataPath (if it starts with it) with $ in the path, so it becomes relative to the parent context
				const path = ReplaceStartOfPath(msg.path, '$', this.#baseDataPath!);
				if (path === undefined) {
					throw new Error('Path was not transformed correctly and can therefor not be synced with parent messages.');
				}
				// Notice, the parent message uses the same key. [NL]
				this.#parent!.messages.addMessage(msg.type, path, msg.body, msg.key);
			});
		}

		this.#parent!.messages.finishChange();
	};

	override hostConnected(): void {
		super.hostConnected();
		this.#readyToSync();
	}
	override hostDisconnected(): void {
		super.hostDisconnected();
		if (this.#parent) {
			this.#parent.removeValidator(this);
		}
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
		if (validator === this) {
			throw new Error('Cannot add it self as validator');
		}
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
				this.validate().catch(() => undefined);
			}
		}
	}

	/**
	 * Validate this context, all the validators of this context will be validated.
	 * Notice its a recursive check meaning sub validation contexts also validates their validators.
	 * @returns succeed {Promise<boolean>} - Returns a promise that resolves to true if the validation succeeded.
	 */
	async validate(): Promise<void> {
		this.#validationMode = true;

		const resultsStatus =
			this.#validators.length === 0
				? true
				: await Promise.all(this.#validators.map((v) => v.validate())).then(
						() => true,
						() => false,
					);

		if (this.#validators.length === 0 && resultsStatus === false) {
			throw new Error('No validators to validate, but validation failed');
		}

		if (this.messages === undefined) {
			// This Context has been destroyed while is was validating, so we should not continue. [NL]
			return Promise.reject();
		}

		// We need to ask again for messages, as they might have been added during the validation process. [NL]
		const hasMessages = this.messages.getHasAnyMessages();

		// If we have any messages then we are not valid, otherwise lets check the validation results: [NL]
		// This enables us to keep client validations though UI is not present anymore — because the client validations got defined as messages. [NL]
		const isValid = hasMessages ? false : resultsStatus;

		this.#isValid = isValid;

		if (isValid === false) {
			if (hasMessages === false && resultsStatus === false) {
				const notValidValidators = this.#validators.filter((v) => v.isValid === false);
				console.warn(
					'Missing validation messages to represent why a child validation context is invalid. These Validators was not valid, one of these did not set a message to represent their state:',
					notValidValidators,
				);
			}
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
		this.messages.clear();
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
		this.#validationMode = false;
		if (this.#inUnprovidingState === true) {
			return;
		}
		this.#destroyValidators();
		this.unprovide();
		this.messages?.destroy();
		(this.messages as unknown) = undefined;
		if (this.#parent) {
			this.#parent.removeValidator(this);
		}
		this.#localMessages = undefined;
		this.#parentMessages = undefined;
		this.#parent = undefined;
		super.destroy();
	}
}
