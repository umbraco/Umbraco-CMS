import type { UmbItemRepository } from './item/index.js';
import { UmbDeprecation } from '@umbraco-cms/backoffice/utils';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { type ManifestRepository, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbExtensionApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UmbEntityUpdatedEvent } from '@umbraco-cms/backoffice/entity-action';

const ObserveRepositoryAlias = Symbol();

interface UmbRepositoryItemsStatus {
	state: {
		type: 'success' | 'error' | 'loading';
		error?: string;
	};
	unique: string;
}

export class UmbRepositoryItemsManager<ItemType extends { unique: string }> extends UmbControllerBase {
	//
	repository?: UmbItemRepository<ItemType>;
	#getUnique: (entry: ItemType) => string | undefined;

	#init: Promise<unknown>;
	#currentRequest?: Promise<unknown>;
	#eventContext?: typeof UMB_ACTION_EVENT_CONTEXT.TYPE;

	// the init promise is used externally for recognizing when the manager is ready.
	public get init() {
		return this.#init;
	}

	#uniques = new UmbArrayState<string>([], (x) => x);
	uniques = this.#uniques.asObservable();

	#items = new UmbArrayState<ItemType>([], (x) => this.#getUnique(x));
	items = this.#items.asObservable();

	#statuses = new UmbArrayState<UmbRepositoryItemsStatus>([], (x) => x.unique);
	statuses = this.#statuses.asObservable();

	// TODO: Align with the other manager(details), and make a generic type/base for these. v.17.0 [NL]
	/**
	 * Creates an instance of UmbRepositoryItemsManager.
	 * @param {UmbControllerHost} host - The host for the controller.
	 * @param {string} repositoryAlias - The alias of the repository to use.
	 * @param {((entry: ItemType) => string | undefined)} [getUniqueMethod] - DEPRECATED since 15.3. Will be removed in v.17.0: A method to get the unique key from the item.
	 * @memberof UmbRepositoryItemsManager
	 */
	constructor(
		host: UmbControllerHost,
		repositoryAlias: string,
		getUniqueMethod?: (entry: ItemType) => string | undefined,
	) {
		super(host);

		this.#getUnique = getUniqueMethod
			? (entry: ItemType) => {
					new UmbDeprecation({
						deprecated: 'The getUniqueMethod parameter.',
						removeInVersion: '17.0.0',
						solution: 'The required unique property on the item will be used instead.',
					}).warn();
					return getUniqueMethod(entry);
				}
			: (entry) => entry.unique;

		this.#init = new UmbExtensionApiInitializer<ManifestRepository<UmbItemRepository<ItemType>>>(
			this,
			umbExtensionsRegistry,
			repositoryAlias,
			[this],
			(permitted, repository) => {
				this.repository = permitted ? repository.api : undefined;
			},
		).asPromise();

		this.observe(
			this.uniques,
			(uniques) => {
				if (uniques.length === 0) {
					this.#items.setValue([]);
					return;
				}

				// TODO: This could be optimized so we only load the appended items, but this requires that the response checks that an item is still present in uniques. [NL]
				// Check if we already have the items, and then just sort them:
				const items = this.#items.getValue();
				if (
					uniques.length === items.length &&
					uniques.every((unique) => items.find((item) => this.#getUnique(item) === unique))
				) {
					this.#items.setValue(this.#sortByUniques(items));
				} else {
					// We need to load new items, so ...
					this.#requestItems();
				}
			},
			null,
		);

		this.consumeContext(UMB_ACTION_EVENT_CONTEXT, (context) => {
			this.#eventContext?.removeEventListener(
				UmbEntityUpdatedEvent.TYPE,
				this.#onEntityUpdatedEvent as unknown as EventListener,
			);

			this.#eventContext = context;
			this.#eventContext.addEventListener(
				UmbEntityUpdatedEvent.TYPE,
				this.#onEntityUpdatedEvent as unknown as EventListener,
			);
		});
	}

	getUniques(): Array<string> {
		return this.#uniques.getValue();
	}

	setUniques(uniques: string[] | undefined): void {
		this.#uniques.setValue(uniques ?? []);
	}

	getItems(): Array<ItemType> {
		return this.#items.getValue();
	}

	itemByUnique(unique: string) {
		return this.#items.asObservablePart((items) => items.find((item) => this.#getUnique(item) === unique));
	}

	async getItemByUnique(unique: string) {
		// TODO: Make an observeOnce feature, to avoid this amount of code: [NL]
		const ctrl = this.observe(this.itemByUnique(unique), () => {}, null);
		const result = await ctrl.asPromise();
		ctrl.destroy();
		return result;
	}

	async #requestItems(): Promise<void> {
		await this.#init;
		if (!this.repository) throw new Error('Repository is not initialized');

		const requestedUniques = this.getUniques();

		this.#statuses.setValue(
			requestedUniques.map((unique) => ({
				state: {
					type: 'loading',
				},
				unique,
			})),
		);

		// TODO: Test if its just some items that is gone now, if so then just filter them out. (maybe use code from #removeItem)
		// This is where this.#getUnique comes in play. Unless that can come from the repository, but that collides with the idea of having a multi-type repository. If that happens.
		const request = this.repository.requestItems(requestedUniques);
		this.#currentRequest = request;
		const { asObservable, data, error } = await request;

		if (this.#currentRequest !== request) {
			// You are not the newest request, so please back out.
			return;
		}

		if (error) {
			this.#statuses.append(
				requestedUniques.map((unique) => ({
					state: {
						type: 'error',
						error: '#general_error',
					},
					unique,
				})),
			);
			return;
		}

		// find uniques not resolved:
		if (data) {
			// find rejected uniques:
			const rejectedUniques = requestedUniques.filter(
				(unique) => !data.find((item) => this.#getUnique(item) === unique),
			);
			const resolvedUniques = requestedUniques.filter((unique) => !rejectedUniques.includes(unique));
			this.#items.remove(rejectedUniques);

			this.#statuses.append([
				...rejectedUniques.map(
					(unique) =>
						({
							state: {
								type: 'error',
								error: '#general_notFound',
							},
							unique,
						}) as UmbRepositoryItemsStatus,
				),
				...resolvedUniques.map(
					(unique) =>
						({
							state: {
								type: 'success',
							},
							unique,
						}) as UmbRepositoryItemsStatus,
				),
			]);
		}

		if (asObservable) {
			this.observe(
				asObservable(),
				(data) => {
					this.#items.setValue(this.#sortByUniques(data));
				},
				ObserveRepositoryAlias,
			);
		}
	}

	async #reloadItem(unique: string): Promise<void> {
		await this.#init;
		if (!this.repository) throw new Error('Repository is not initialized');

		const { data, error } = await this.repository.requestItems([unique]);

		if (error) {
			this.#statuses.appendOne({
				state: {
					type: 'error',
					error: '#general_notFound',
				},
				unique,
			} as UmbRepositoryItemsStatus);
		}

		if (data) {
			const items = this.getItems();
			const item = items.find((item) => this.#getUnique(item) === unique);

			if (item) {
				const index = items.indexOf(item);
				const newItems = [...items];
				newItems[index] = data[0];
				this.#items.setValue(this.#sortByUniques(newItems));
			}
		}
	}

	#sortByUniques(data: Array<ItemType>): Array<ItemType> {
		const uniques = this.getUniques();
		return [...data].sort((a, b) => {
			const aIndex = uniques.indexOf(this.#getUnique(a) ?? '');
			const bIndex = uniques.indexOf(this.#getUnique(b) ?? '');
			return aIndex - bIndex;
		});
	}

	#onEntityUpdatedEvent = (event: UmbEntityUpdatedEvent) => {
		const eventUnique = event.getUnique();

		const items = this.getItems();
		if (items.length === 0) return;

		// Ignore events if the entity is not in the list of items.
		const item = items.find((item) => this.#getUnique(item) === eventUnique);
		if (!item) return;

		this.#reloadItem(item.unique);
	};

	override destroy(): void {
		this.#eventContext?.removeEventListener(
			UmbEntityUpdatedEvent.TYPE,
			this.#onEntityUpdatedEvent as unknown as EventListener,
		);
		super.destroy();
	}
}
