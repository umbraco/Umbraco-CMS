import { extractJsonQueryProps } from '../utils/extract-json-query-properties.function.js';
import { umbGetFirstJsonPathBracket } from '../utils/first-json-path-bracket.function.js';
import type { UmbValidationController } from './validation.controller.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { Observable } from '@umbraco-cms/backoffice/observable-api';

/**
 * A method resolving the unique identifier of a message's first-bracket query parameters, e.g. for
 * `{alias: 'title', culture: 'en-US'}` this might return `'title'`. Return undefined to ignore a message
 * (it won't be considered for removal, regardless of what disappears from the known uniques).
 */
export type UmbValidationCleanUpGetUniqueMethod = (queryParams: Record<string, string>) => string | undefined;

/**
 * Removes the validation messages of items based on observation of uniques.
 *
 * @example
 * ```ts
 * new UmbValidationCleanUpByUniqueManager(
 * 	this,
 * 	this.validationContext,
 * 	'$.values',
 * 	this.structure.contentTypePropertyAliases,
 * 	(queryParams) => queryParams.alias,
 * );
 * ```
 */
export class UmbValidationCleanUpByUniqueManager extends UmbControllerBase {
	readonly #validation: UmbValidationController;
	readonly #scopePath: string;
	readonly #getUniqueMethod: UmbValidationCleanUpGetUniqueMethod;

	/**
	 * The uniques known the last time the observed uniques were handled.
	 * Undefined until the first emission has been handled. [NL]
	 */
	#knownUniques?: Set<string>;

	/**
	 * Creates an instance of UmbValidationCleanUpByUniqueManager.
	 * @param {UmbControllerHost} host - The host of this controller.
	 * @param {UmbValidationController} validationController - The Validation Context to remove messages from.
	 * @param {string} scopePath - The JSON-Path prefix to scan for messages, e.g. `$.values`.
	 * @param {Observable<Array<string>>} uniques - An Observable of the currently known unique identifiers.
	 * @param {UmbValidationCleanUpGetUniqueMethod} getUniqueMethod - Resolves the unique identifier of a message's first-bracket query parameters.
	 */
	constructor(
		host: UmbControllerHost,
		validationController: UmbValidationController,
		scopePath: string,
		uniques: Observable<Array<string> | undefined>,
		getUniqueMethod: UmbValidationCleanUpGetUniqueMethod,
	) {
		super(host);
		this.#validation = validationController;
		this.#scopePath = scopePath;
		this.#getUniqueMethod = getUniqueMethod;

		this.observe(uniques, this.#gotUniques, null);
	}

	#gotUniques = (uniques: Array<string> | undefined): void => {
		const currentUniques = new Set(uniques ?? []);
		const knownUniques = this.#knownUniques;
		this.#knownUniques = currentUniques;

		// The first emission only establishes the baseline — nothing was "removed" before we knew about it: [NL]
		if (knownUniques === undefined) return;

		const removedUniques = new Set<string>();
		for (const unique of knownUniques) {
			if (!currentUniques.has(unique)) {
				removedUniques.add(unique);
			}
		}
		if (removedUniques.size === 0) return;

		// The Validation Context may already have been destroyed, then there is nothing to clean up: [NL]
		const messages = this.#validation.messages;
		if (!messages) return;

		const keysToRemove: Array<string> = [];
		for (const message of messages.getMessagesOfPathAndDescendant(this.#scopePath)) {
			const bracket = umbGetFirstJsonPathBracket(message.path);
			if (bracket === undefined) continue;

			const unique = this.#getUniqueMethod(extractJsonQueryProps(bracket));
			if (unique !== undefined && removedUniques.has(unique)) {
				keysToRemove.push(message.key);
			}
		}

		messages.removeMessageByKeys(keysToRemove);
	};

	override destroy(): void {
		super.destroy();
		this.#knownUniques = undefined;
		(this.#validation as unknown) = undefined;
	}
}
