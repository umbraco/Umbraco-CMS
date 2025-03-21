import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { type ClassConstructor } from '@umbraco-cms/backoffice/extension-api';
import type { UmbValidationMessage } from '../../context/validation-messages.manager.js';
import type { UmbValidationPathTranslator } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export type UmbValidationTranslationControllerArgs = {
	pathTranslators: Array<ClassConstructor<UmbValidationPathTranslator<any>>>;
	translationData: unknown;
};

// Write interface that can be handed to the API for the Host, so each Path Translator can communicate back to the host here. For translating inner values.
export class UmbValidationPathTranslationController extends UmbControllerBase {
	//
	#pathTranslators: Array<ClassConstructor<UmbValidationPathTranslator<any>>> = [];
	#data: unknown;

	constructor(host: UmbControllerHost, args: UmbValidationTranslationControllerArgs) {
		super(host);

		this.#pathTranslators = args.pathTranslators;
		this.#data = args.translationData;
	}

	async translate(messages: Array<UmbValidationMessage>): Promise<UmbValidationMessage[]> {
		let paths = messages.map((msg) => msg.path);

		for (const translatorClass of this.#pathTranslators) {
			const translator = new translatorClass(this);
			// Here we can call the translate method on the translator, and pass the translationData to it.
			paths = await translator.translate(paths, this.#data);

			translator.destroy();

			// merge messages with the paths, matching index:
			messages = messages.map((msg, i) => {
				return {
					...msg,
					// Here we can use the order/index to map back to the message-object.
					path: paths[i],
				};
			});
		}

		return messages;
	}
}
