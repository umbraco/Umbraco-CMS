import type { UmbItemRepository } from './item/index.js';
import type { UmbRepositoryItemsStatus } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { type ManifestRepository, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbExtensionApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UmbEntityUpdatedEvent } from '@umbraco-cms/backoffice/entity-action';
import { UmbDeprecation } from '@umbraco-cms/backoffice/utils';

const ObserveRepositoryAlias = Symbol();

export class UmbRepositoryItemsManager<ItemType extends { unique: string }> extends UmbControllerBase {
	//
	repository?: UmbItemRepository<ItemType>;

	#init: Promise<unknown>;
	#initResolve!: (value: unknown) => void;

	#currentRequest?: Promise<unknown>;
	#eventContext?: typeof UMB_ACTION_EVENT_CONTEXT.TYPE;

	// the init promise is used externally for recognizing when the manager is ready.
	public get init() {
		return this.#init;
	}

	#uniques = new UmbArrayState<string>([], (x) => x);
	uniques = this.#uniques.asObservable();

	#items = new UmbArrayState<ItemType>([], (x) => x.unique);
	items = this.#items.asObservable();

	#statuses = new UmbArrayState<UmbRepositoryItemsStatus>([], (x) => x.unique);
	statuses = this.#statuses.asObservable();

	// TODO: Align with the other manager(details), and make a generic type/base for these. v.17.0 [NL]
	/**
	 * Creates an instance of UmbRepositoryItemsManager.
	 * @param {UmbControllerHost} host - The host for the controller.
	 * @param {string} repositoryAlias - The alias of the repository to use.
	 * @memberof UmbRepositoryItemsManager
	 */
	constructor(host: UmbControllerHost, repositoryAlias?: string) {
		super(host);

		this.#init = new Promise((resolve) => {
			this.#initResolve = resolve;
		});

		if (repositoryAlias) {
			this.#initItemRepository(repositoryAlias);
		}

		this.observe(
			this.uniques,
			(uniques) => {
				if (uniques.length === 0) {
					this.#items.setValue([]);
					this.#statuses.setValue([]);
					return;
				}

				// TODO: This could be optimized so we only load the appended items, but this requires that the response checks that an item is still present in uniques. [NL]
				// Check if we already have the statuses, and then just sort them:
				const statuses = this.#statuses.getValue();
				if (
					uniques.length === statuses.length &&
					uniques.every((unique) => statuses.find((status) => status.unique === unique))
				) {
					const items = this.#items.getValue();
					this.#items.setValue(this.#sortByUniques(items));
					this.#statuses.setValue(this.#sortByUniques(statuses));
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
			this.#eventContext?.addEventListener(
				UmbEntityUpdatedEvent.TYPE,
				this.#onEntityUpdatedEvent as unknown as EventListener,
			);
		});
	}

	/**
	 * Sets the item repository to use for this manager.
	 * @param {(UmbItemRepository<ItemType> | undefined)} itemRepository - The item repository to set.
	 * @memberof UmbRepositoryItemsManager
	 */
	setItemRepository(itemRepository: UmbItemRepository<ItemType> | undefined) {
		this.repository = itemRepository;
		this.#initResolve(undefined);
	}

	/**
	 * Gets the item repository used by this manager.
	 * @returns {(UmbItemRepository<ItemType> | undefined)} The item repository.
	 * @memberof UmbRepositoryItemsManager
	 */
	getItemRepository(): UmbItemRepository<ItemType> | undefined {
		return this.repository;
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
		return this.#items.asObservablePart((items) => items.find((item) => item.unique === unique));
	}

	/**
	 * @deprecated - This is resolved by setUniques, no need to update statuses.
	 * @param unique {string} - The unique identifier of the item to remove the status of.
	 */
	removeStatus(unique: string) {
		new UmbDeprecation({
			removeInVersion: '18.0.0',
			deprecated: 'removeStatus',
			solution: 'Statuses are removed automatically when setting uniques',
		}).warn();
		this.#statuses.filter((status) => status.unique !== unique);
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
			// No need to do sorting here as we just got the unique in the right order above.
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
			this.#statuses.replace(
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
			const rejectedUniques = requestedUniques.filter((unique) => !data.find((item) => item.unique === unique));
			const resolvedUniques = requestedUniques.filter((unique) => !rejectedUniques.includes(unique));
			this.#items.remove(rejectedUniques);

			this.#statuses.replace([
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
		} else if (data) {
			this.#items.setValue(data);
		}
	}

	async #reloadItem(unique: string): Promise<void> {
		await this.#init;
		if (!this.repository) throw new Error('Repository is not initialized');

		const { data, error } = await this.repository.requestItems([unique]);

		if (error) {
			this.#statuses.updateOne(unique, {
				state: {
					type: 'error',
					error: '#general_notFound',
				},
			} as UmbRepositoryItemsStatus);
		}

		if (data) {
			const items = this.getItems();
			const item = items.find((item) => item.unique === unique);

			if (item) {
				const index = items.indexOf(item);
				const newItems = [...items];
				newItems[index] = data[0];
				this.#items.setValue(this.#sortByUniques(newItems));
				// No need to update statuses here, as the item is the same, just updated.
			}
		}
	}

	#sortByUniques<T extends Pick<ItemType, 'unique'>>(data?: Array<T>): Array<T> {
		if (!data) return [];
		const uniques = this.getUniques();
		return [...data].sort((a, b) => {
			const aIndex = uniques.indexOf(a.unique ?? '');
			const bIndex = uniques.indexOf(b.unique ?? '');
			return aIndex - bIndex;
		});
	}

	#onEntityUpdatedEvent = (event: UmbEntityUpdatedEvent) => {
		const eventUnique = event.getUnique();

		const items = this.getItems();
		if (items.length === 0) return;

		// Ignore events if the entity is not in the list of items.
		const item = items.find((item) => item.unique === eventUnique);
		if (!item) return;

		this.#reloadItem(item.unique);
	};

	async #initItemRepository(itemRepositoryAlias: string) {
		new UmbExtensionApiInitializer<ManifestRepository<UmbItemRepository<ItemType>>>(
			this,
			umbExtensionsRegistry,
			itemRepositoryAlias,
			[this],
			(permitted, repository) => {
				this.repository = permitted ? repository.api : undefined;
				this.#initResolve(undefined);
			},
		);
	}

	override destroy(): void {
		this.#eventContext?.removeEventListener(
			UmbEntityUpdatedEvent.TYPE,
			this.#onEntityUpdatedEvent as unknown as EventListener,
		);
		super.destroy();
	}
}
