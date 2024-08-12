import { UmbContextProviderController } from '@umbraco-cms/backoffice/context-api';
import type { UmbValidator } from '../interfaces/validator.interface.js';
import { type UmbValidationMessage, UmbValidationMessagesManager } from './validation-messages.manager.js';
import { UMB_VALIDATION_CONTEXT } from './validation.context-token.js';
import { type UmbClassInterface, UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { type UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbValidationMessageTranslator } from '../translators/index.js';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { GetValueByJsonPath } from '../utils/json-path.function.js';

function ReplaceStartOfString(path: string, startFrom: string, startTo: string): string {
	if (path.startsWith(startFrom + '.')) {
		return startTo + path.slice(startFrom.length);
	}
	return path;
}

export class UmbValidationContext extends UmbControllerBase implements UmbValidator {
	// The current provider controller, that is providing this context:
	#providerCtrl?: UmbContextProviderController<UmbValidationContext, UmbValidationContext, UmbValidationContext>;

	// Local version of the data send to the server, only use-case is for translation.
	#translationData = new UmbObjectState<any>(undefined);
	translationDataOf(path: string): any {
		console.log('GetValueByJsonPath', path);
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

	#parent?: UmbValidationContext;
	#parentMessages?: Array<UmbValidationMessage>;
	#localMessages?: Array<UmbValidationMessage>;
	#baseDataPath?: string;

	public readonly messages = new UmbValidationMessagesManager();

	constructor(host: UmbControllerHost) {
		// This is overridden to avoid setting a controllerAlias, this might make sense, but currently i want to leave it out. [NL]
		super(host);
	}

	async addTranslator(translator: UmbValidationMessageTranslator) {
		this.messages.addTranslator(translator);
	}

	async removeTranslator(translator: UmbValidationMessageTranslator) {
		this.messages.removeTranslator(translator);
	}

	/**
	 * Provides the validation context to the current host, if not already provided to a different host.
	 * @returns instance {UmbValidationContext} - Returns it self.
	 */
	provide(): UmbValidationContext {
		if (this.#providerCtrl) return this;
		this.provideContext(UMB_VALIDATION_CONTEXT, this);
		return this;
	}
	provideAt(controllerHost: UmbClassInterface): void {
		this.#providerCtrl?.destroy();
		this.#providerCtrl = controllerHost.provideContext(UMB_VALIDATION_CONTEXT, this);
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

			this.messages.clear();

			console.log('observe path', dataPath);
			this.observe(parent.translationDataOf(dataPath), (data) => {
				console.log('got data', data);
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
						// TODO: Subtract the base path from the path, so it becomes local to this context:
						const path = ReplaceStartOfString(msg.path, this.#baseDataPath!, '$');
						// Notice, the local message uses the same key. [NL]
						this.messages.addMessage(msg.type, path, msg.message, msg.key);
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
						// TODO: Prefix the base path from our base path, so it fits in the parent context:
						// replace this.#baseDataPath (if it starts with it) with $ in the path, so it becomes relative to the parent context
						const path = ReplaceStartOfString(msg.path, '$', this.#baseDataPath!);
						// Notice, the parent message uses the same key. [NL]
						this.#parent!.messages.addMessage(msg.type, path, msg.message, msg.key);
					});
				},
				'observeLocalMessages',
			);

			// observe if one of the locals got removed.
			// It can maybe be done with one set of known/gotten parent messages, that then can be used to detect which are removed. Maybe from both sides.

			// Benefits for workspaces:
			// — The workspace can be validated locally, and then the messages can propagate to parent context. (Do we even want that?)
			// — The workspace can easier know about its validation state
			// — Its the only way the sub-workspace can be validated without triggering the whole validation.
			// - The workspace can inherit messages from parent context... — which is good for Blocks
			// - The workspace can have its own server messages, that is propagated to parent context.
			// - The workspace can inherit server messages from parent context... — server validation of a block, that is part of the parent workspace.
			// - Remove parent messages if they go away again if they gets remove here.
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
