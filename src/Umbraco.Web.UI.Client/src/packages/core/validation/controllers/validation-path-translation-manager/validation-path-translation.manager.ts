import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { createExtensionApi } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbValidationMessage } from '../../context/validation-messages.manager.js';
import type { UmbPropertyValueDataPotentiallyWithEditorAlias } from '@umbraco-cms/backoffice/property';
import type { UmbValidationPathTranslator } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { GetValueByJsonPath } from '../../utils/json-path.function.js';

export type UmbValidationTranslatorControllerArgs = {
	pathTranslators: Array<UmbValidationPathTranslator>;
	translationData: unknown;
};

// Write interface that can be handed to the API for the Host, so each path Translator can communicate back to the host here. For translating inner values.
export class UmbValidationTranslatorController extends UmbControllerBase {
	//
	#pathTranslators: Array<UmbValidationPathTranslator> = [];
	#data: unknown;

	constructor(host: UmbControllerHost, args: UmbValidationTranslatorControllerArgs) {
		super(host);

		this.#pathTranslators = args.pathTranslators;
		this.#data = args.translationData;
	}

	async translate(messages: Array<UmbValidationMessage>): Promise<UmbValidationMessage[]> {
		let paths = messages.map((msg) => msg.path);

		for (const translator of this.#pathTranslators) {
			// Here we can call the translate method on the translator, and pass the translationData to it.
			paths = await translator.translate(paths, this.#data);

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

	/**
	 * If not parsing one at a time, then we need a #translatePaths method,
	 * this takes an array of paths and returns an array of translated paths, most likely returning all, some translated the onces it cannot handle are not and just returned as is.
	 * This goes on with the values, all the paths that matches a base-path should be parse to the translatePaths
	 */
	/**
	 * translates the property data.
	 * @param {UmbPropertyValueDataPotentiallyWithEditorAlias} property - The property data.
	 * @returns {Promise<UmbPropertyValueDataPotentiallyWithEditorAlias>} - A promise that resolves to the cloned property data.
	 */
	async translatePropertiesArray(
		propertiesPaths: Array<string>,
		propertitesData: Array<UmbPropertyValueDataPotentiallyWithEditorAlias>,
	): Promise<Array<string>> {
		// propertyPaths is like ['[0].value', '[0].value.something', '[1].value'] group them by the first index:
		const pathsGrouped = propertiesPaths.reduce(
			(acc, path) => {
				// This could already be translated, meaning it can both be an index or a JSON-Path query.
				const pathSplit = path.split(']', 1);
				const indexOrQuery = pathSplit[0].substring(1);
				acc[indexOrQuery] = acc[indexOrQuery] || [];
				acc[indexOrQuery].push(pathSplit[1]);
				return acc;
			},
			{} as Record<string, Array<string>>,
		);

		const promises: Array<Promise<string[]>> = [];

		for (const indexOrQuery in pathsGrouped) {
			const numberIndex = Number(indexOrQuery);
			let data: UmbPropertyValueDataPotentiallyWithEditorAlias<unknown>;
			if (isNaN(numberIndex)) {
				data = GetValueByJsonPath(
					propertitesData,
					`$[${indexOrQuery}]`,
				) as UmbPropertyValueDataPotentiallyWithEditorAlias<unknown>;
			} else {
				data = propertitesData[numberIndex];
			}
			const paths = pathsGrouped[indexOrQuery];
			promises.push(this.translateProperty(paths, data));
		}

		// We will first await the promises here, so they don't have to wait on each other but can all run simultaneously. [NL]
		const translated = await Promise.all(promises);
		const paths = translated.flatMap((x) => x);
		return paths;
	}

	/**
	 * translates the property data.
	 * @param {UmbPropertyValueDataPotentiallyWithEditorAlias} property - The property data.
	 * @returns {Promise<UmbPropertyValueDataPotentiallyWithEditorAlias>} - A promise that resolves to the cloned property data.
	 */
	async translateProperty(
		propertyPaths: Array<string>,
		propertyData: UmbPropertyValueDataPotentiallyWithEditorAlias,
	): Promise<Array<string>> {
		const editorAlias = propertyData.editorAlias;
		if (!editorAlias) {
			console.error(`Editor alias not found for ${propertyData.alias}`);
			return propertyPaths;
		}

		// Find the cloner for this editor alias:
		const manifest = umbExtensionsRegistry.getByTypeAndFilter(
			'propertyValidationPathTranslator',
			(x) => x.forEditorAlias === editorAlias,
		)[0];

		if (!manifest) {
			return propertyPaths;
		}

		const api = await createExtensionApi(this, manifest);
		if (!api) {
			return propertyPaths;
		}

		propertyPaths = (await api.translate(propertyPaths, propertyData)) ?? propertyPaths;

		return propertyPaths;
	}
}
