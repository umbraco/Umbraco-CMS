import type { UmbDetailRepository } from './detail/detail-repository.interface.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState, type Observable } from '@umbraco-cms/backoffice/observable-api';
import { type ManifestRepository, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbExtensionApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UmbEntityUpdatedEvent } from '@umbraco-cms/backoffice/entity-action';

interface UmbRepositoryRequestStatus {
	state: {
		type: 'success' | 'error' | 'loading';
		error?: string;
	};
	unique: string;
}

/**
 * @export
 * @class UmbRepositoryDetailsManager
 * @augments {UmbControllerBase}
 * @template DetailType
 */
export class UmbRepositoryDetailsManager<DetailType extends { unique: string }> extends UmbControllerBase {
	//
	repository?: UmbDetailRepository<DetailType>;

	#init: Promise<unknown>;
	#eventContext?: typeof UMB_ACTION_EVENT_CONTEXT.TYPE;

	// the init promise is used externally for recognizing when the manager is ready.
	public get init() {
		return this.#init;
	}

	#uniques = new UmbArrayState<DetailType['unique']>([], (x) => x);
	uniques = this.#uniques.asObservable();

	#entries = new UmbArrayState<DetailType, DetailType['unique']>([], (x) => x.unique);
	entries = this.#entries.asObservable();

	#statuses = new UmbArrayState<UmbRepositoryRequestStatus>([], (x) => x.unique);
	statuses = this.#statuses.asObservable();

	/**
	 * Creates an instance of UmbRepositoryDetailsManager.
	 * @param {UmbControllerHost} host - The host for the controller.
	 * @param {string} repository - The alias of the repository to use.
	 * @memberof UmbRepositoryDetailsManager
	 */
	constructor(host: UmbControllerHost, repository: UmbDetailRepository<DetailType> | string) {
		super(host);

		this.#entries.sortBy((a, b) => {
			const uniques = this.getUniques();
			const aIndex = uniques.indexOf(a.unique);
			const bIndex = uniques.indexOf(b.unique);
			return aIndex - bIndex;
		});

		if (typeof repository === 'string') {
			this.#init = new UmbExtensionApiInitializer<ManifestRepository<UmbDetailRepository<DetailType>>>(
				this,
				umbExtensionsRegistry,
				repository,
				[this],
				(permitted, repository) => {
					this.repository = permitted ? repository.api : undefined;
				},
			).asPromise();
		} else {
			this.repository = repository;
			this.#init = Promise.resolve();
		}

		this.observe(
			this.uniques,
			(uniques) => {
				// remove entries based on no-longer existing uniques:
				const removedEntries = this.#entries
					.getValue()
					.filter((entry) => !uniques.includes(entry.unique))
					.map((x) => x.unique);

				this.#statuses.remove(removedEntries);
				this.#entries.remove(removedEntries);
				removedEntries.forEach((entry) => {
					this.removeUmbControllerByAlias('observeEntry_' + entry);
				});

				this.#requestNewDetails(uniques);
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
	 * Clear the manager
	 * @memberof UmbRepositoryDetailsManager
	 */
	clear(): void {
		this.#uniques.setValue([]);
		this.#entries.setValue([]);
		this.#statuses.setValue([]);
	}

	/**
	 * Get the uniques in the manager
	 * @returns {Array<string>} - The uniques in the manager.
	 * @memberof UmbRepositoryDetailsManager
	 */
	getUniques(): Array<string> {
		return this.#uniques.getValue();
	}

	/**
	 * Set the uniques in the manager
	 * @param {(string[] | undefined)} uniques
	 * @memberof UmbRepositoryDetailsManager
	 */
	setUniques(uniques: Array<string> | undefined): void {
		this.#uniques.setValue(uniques ?? []);
	}

	/**
	 * Add a unique to the manager
	 * @param {string} unique
	 * @memberof UmbRepositoryDetailsManager
	 */
	addUnique(unique: string): void {
		this.#uniques.appendOne(unique);
	}

	/**
	 * Add an entry to the manager
	 * @param {DetailType} data
	 * @memberof UmbRepositoryDetailsManager
	 */
	addEntry(data: DetailType): void {
		const unique = data.unique;
		this.#entries.appendOne(data);
		// Add status BEFORE unique to prevent #requestNewDetails from trying to fetch
		// (the observation callback on #uniques checks #statuses to skip already-loaded entries)
		this.#statuses.appendOne({
			state: {
				type: 'success',
			},
			unique,
		});
		this.#uniques.appendOne(unique);
		// Notice in this case we do not have a observable from the repo, but it should maybe be fine that we just listen for ACTION EVENTS.
	}

	/**
	 * Get all entries in the manager
	 * @returns {Array<DetailType>} - The entries in the manager.
	 * @memberof UmbRepositoryDetailsManager
	 */
	getEntries(): Array<DetailType> {
		return this.#entries.getValue();
	}

	/**
	 * Get an entry observable by unique
	 * @param {string} unique
	 * @returns {Observable<DetailType | undefined>} - The entry observable.
	 * @memberof UmbRepositoryDetailsManager
	 */
	entryByUnique(unique: string): Observable<DetailType | undefined> {
		return this.#entries.asObservablePart((items) => items.find((item) => item.unique === unique));
	}

	async #requestNewDetails(uniques?: Array<DetailType['unique']>): Promise<void> {
		if (!uniques?.length) return;

		await this.#init;
		if (!this.repository) throw new Error('Repository is not initialized');

		const newRequestedUniques = uniques.filter((unique) => {
			const item = this.#statuses.getValue().find((status) => status.unique === unique);
			return !item;
		});

		if (newRequestedUniques.length === 0) return;

		// Use bulk endpoint when available, fall back to individual requests
		if (this.repository.requestByUniques) {
			this.#requestDetailsBulk(newRequestedUniques);
		} else {
			newRequestedUniques.forEach((unique) => {
				this.#requestDetails(unique);
			});
		}
	}

	async #requestDetailsBulk(uniques: Array<DetailType['unique']>): Promise<void> {
		await this.#init;
		if (!this.repository?.requestByUniques) throw new Error('Repository does not support requestByUniques');

		for (const unique of uniques) {
			this.#setStatus(unique, 'loading');
		}

		const { data, error, asObservable } = await this.repository.requestByUniques(uniques);

		if (error) {
			for (const unique of uniques) {
				this.#setError(unique);
			}
			return;
		}

		if (data) {
			const currentUniques = this.getUniques();

			for (const unique of uniques) {
				// Check it still exists in uniques (may have been removed while request was in flight)
				if (!currentUniques.includes(unique)) {
					this.#statuses.removeOne(unique);
					continue;
				}

				const item = data.find((d) => d.unique === unique);
				if (item) {
					this.#setSuccess(unique, item);
				} else {
					// Item was requested but not returned — treat as not found
					this.#setError(unique);
				}
			}

			// Set up per-item store observation for reactivity
			const successUniques = uniques.filter((u) => currentUniques.includes(u) && data.some((d) => d.unique === u));
			for (const unique of successUniques) {
				const observable = asObservable?.();
				if (observable) {
					this.observe(
						observable,
						(items) => {
							const item = items?.find((d) => d.unique === unique);
							if (item) {
								this.#entries.updateOne(unique, item);
							} else {
								this.#entries.removeOne(unique);
							}
						},
						'observeEntry_' + unique,
					);
				}
			}
		}
	}

	async #reloadDetails(unique: string): Promise<void> {
		return await this.#requestDetails(unique);
	}

	async #requestDetails(unique: string): Promise<void> {
		await this.#init;
		if (!this.repository) throw new Error('Repository is not initialized');

		this.#setStatus(unique, 'loading');

		const { data, error, asObservable } = await this.repository.requestByUnique(unique);

		if (error) {
			this.#setError(unique);
		}

		if (data) {
			// Check it still exists in uniques:
			const uniques = this.getUniques();
			if (!uniques.includes(unique)) {
				this.#statuses.removeOne(unique);
				return;
			}

			this.#setSuccess(unique, data);

			const observable = asObservable?.();
			if (observable) {
				this.#observeEntry(unique, observable);
			}
		}
	}

	#setStatus(unique: string, type: 'success' | 'error' | 'loading'): void {
		this.#statuses.appendOne({
			state: { type },
			unique,
		});
	}

	#setError(unique: string): void {
		this.#statuses.appendOne({
			state: {
				type: 'error',
				error: '#general_notFound',
			},
			unique,
		} as UmbRepositoryRequestStatus);
		this.#entries.removeOne(unique);
		this.removeUmbControllerByAlias('observeEntry_' + unique);
	}

	#setSuccess(unique: string, data: DetailType): void {
		this.#entries.appendOne(data);
		this.#setStatus(unique, 'success');
	}

	#observeEntry(unique: string, observable: Observable<DetailType | undefined>): void {
		this.observe(
			observable,
			(data) => {
				if (data) {
					this.#entries.updateOne(unique, data);
				} else {
					this.#entries.removeOne(unique);
				}
			},
			'observeEntry_' + unique,
		);
	}

	#onEntityUpdatedEvent = (event: UmbEntityUpdatedEvent) => {
		const eventUnique = event.getUnique();

		const items = this.getEntries();
		if (items.length === 0) return;

		// Ignore events if the entity is not in the list of items.
		const item = items.find((item) => item.unique === eventUnique);
		if (!item) return;

		this.#reloadDetails(item.unique);
	};

	override destroy(): void {
		this.#eventContext?.removeEventListener(
			UmbEntityUpdatedEvent.TYPE,
			this.#onEntityUpdatedEvent as unknown as EventListener,
		);
		super.destroy();
	}
}
