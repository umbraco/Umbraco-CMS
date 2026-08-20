import type { UmbValidationController } from './validation.controller.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerAlias, UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { Observable } from '@umbraco-cms/backoffice/observable-api';

/**
 * A method resolving the validation data path of a given item.
 * The path must be a stable identity for the item — it must not change while the item is still present,
 * otherwise it would look like the item was removed and a new one added in its place.
 * Return undefined for items that should be ignored entirely.
 */
export type UmbValidationDataPathResolver<ItemType> = (item: ItemType) => string | undefined;

/**
 * Removes the validation messages of items that disappear from an observed array of items.
 *
 * Each item is identified by the validation data path returned for it by `dataPathResolver`. When an item
 * that was previously present stops being present, the messages at its data path — and at any descendant
 * path — are removed from the given Validation Context. Meaning anything nested inside a removed item's
 * data (e.g. a Block nested inside a removed Block) goes away with it.
 *
 * Notice the first emission of the observed array only establishes the baseline, as it can not be known
 * what was removed before this manager began observing.
 * @example
 * ```ts
 * new UmbValidationCleanUpManager<UmbBlockDataModel>(
 * 	this,
 * 	validationContext,
 * 	this.contents,
 * 	(content) => `$.contentData[?(@.key == '${content.key}')]`,
 * );
 * ```
 */
export class UmbValidationCleanUpManager<ItemType = unknown> extends UmbControllerBase {
	readonly #validation: UmbValidationController;
	readonly #dataPathResolver: UmbValidationDataPathResolver<ItemType>;

	/**
	 * The data paths of the items of the latest emission.
	 * Undefined until the first emission has been handled. [NL]
	 */
	#currentDataPaths?: Set<string>;

	/**
	 * Creates an instance of UmbValidationCleanUpManager.
	 * @param {UmbControllerHost} host - The host of this controller.
	 * @param {UmbValidationController} validationController - The Validation Context to remove messages from.
	 * @param {Observable<Array<ItemType>>} items - An Observable of the items to observe for removals.
	 * @param {UmbValidationDataPathResolver<ItemType>} dataPathResolver - Resolves the validation data path of a given item.
	 * @param {UmbControllerAlias} [controllerAlias] - An optional controller alias, enables replacing this manager with a new one.
	 */
	constructor(
		host: UmbControllerHost,
		validationController: UmbValidationController,
		items: Observable<Array<ItemType>>,
		dataPathResolver: UmbValidationDataPathResolver<ItemType>,
		controllerAlias?: UmbControllerAlias,
	) {
		super(host, controllerAlias);
		this.#validation = validationController;
		this.#dataPathResolver = dataPathResolver;

		this.observe(items, (currentItems) => this.#setItems(currentItems ?? []), null);
	}

	#setItems(items: Array<ItemType>): void {
		const dataPaths = new Set<string>();
		for (const item of items) {
			const dataPath = this.#dataPathResolver(item);
			if (dataPath !== undefined) {
				dataPaths.add(dataPath);
			}
		}

		const currentDataPaths = this.#currentDataPaths;
		this.#currentDataPaths = dataPaths;

		// The first emission only establishes the baseline — nothing was "removed" before we knew about it: [NL]
		if (currentDataPaths === undefined) return;

		const removedDataPaths: Array<string> = [];
		for (const dataPath of currentDataPaths) {
			if (!dataPaths.has(dataPath)) {
				removedDataPaths.push(dataPath);
			}
		}
		if (removedDataPaths.length === 0) return;

		// The Validation Context may already have been destroyed, then there is nothing to clean up: [NL]
		const messages = this.#validation.messages;
		if (!messages) return;

		messages.initiateChange();
		for (const dataPath of removedDataPaths) {
			messages.removeMessagesAndDescendantsByPath(dataPath);
		}
		messages.finishChange();
	}

	override destroy(): void {
		super.destroy();
		this.#currentDataPaths = undefined;
		(this.#validation as unknown) = undefined;
	}
}
