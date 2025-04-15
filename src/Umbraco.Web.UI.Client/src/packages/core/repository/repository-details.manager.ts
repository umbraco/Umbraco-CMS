import type { UmbDetailRepository } from './detail/detail-repository.interface.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { type ManifestRepository, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbExtensionApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UmbEntityUpdatedEvent } from '@umbraco-cms/backoffice/entity-action';

interface UmbRepositoryStatus {
	state: {
		type: 'success' | 'error' | 'loading';
		error?: string;
	};
	unique: string;
}

export class UmbRepositoryDetailsManager<DetailType extends { unique: string }> extends UmbControllerBase {
	//
	repository?: UmbDetailRepository<DetailType>;

	#init: Promise<unknown>;
	#eventContext?: typeof UMB_ACTION_EVENT_CONTEXT.TYPE;

	// the init promise is used externally for recognizing when the manager is ready.
	public get init() {
		return this.#init;
	}

	#uniques = new UmbArrayState<string>([], (x) => x);
	uniques = this.#uniques.asObservable();

	#entries = new UmbArrayState<DetailType>([], (x) => x.unique);
	entries = this.#entries.asObservable();

	#statuses = new UmbArrayState<UmbRepositoryStatus>([], (x) => x.unique);
	statuses = this.#statuses.asObservable();

	/**
	 * Creates an instance of UmbRepositoryItemsManager.
	 * @param {UmbControllerHost} host - The host for the controller.
	 * @param {string} repository - The alias of the repository to use.
	 * @memberof UmbRepositoryItemsManager
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
				const removedEntries = this.#entries.getValue().filter((entry) => !uniques.includes(entry.unique));
				removedEntries.forEach((entry) => {
					const unique = entry.unique;
					if (unique) {
						this.#entries.removeOne(unique);
						this.removeUmbControllerByAlias('observeEntry_' + unique);
					}
				});

				this.#requestNewItems();
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

	getItems(): Array<DetailType> {
		return this.#entries.getValue();
	}

	itemByUnique(unique: string) {
		return this.#entries.asObservablePart((items) => items.find((item) => item.unique === unique));
	}

	async getItemByUnique(unique: string) {
		// TODO: Make an observeOnce feature, to avoid this amount of code: [NL]
		const ctrl = this.observe(this.itemByUnique(unique));
		const result = await ctrl.asPromise();
		ctrl.destroy();
		return result;
	}

	async #requestNewItems(): Promise<void> {
		await this.#init;
		if (!this.repository) throw new Error('Repository is not initialized');

		const requestedUniques = this.getUniques();

		const newRequestedUniques = requestedUniques.filter((unique) => {
			const item = this.#statuses.getValue().find((status) => status.unique === unique);
			return !item;
		});

		newRequestedUniques.forEach((unique) => {
			this.#requestItem(unique);
		});
	}

	async #reloadItem(unique: string): Promise<void> {
		return await this.#requestItem(unique);
	}

	async #requestItem(unique: string): Promise<void> {
		await this.#init;
		if (!this.repository) throw new Error('Repository is not initialized');

		this.#statuses.appendOne({
			state: {
				type: 'loading',
			},
			unique,
		});

		const { data, error, asObservable } = await this.repository.requestByUnique(unique);

		if (error) {
			this.#statuses.appendOne({
				state: {
					type: 'error',
					error: '#general_notFound',
				},
				unique,
			} as UmbRepositoryStatus);
			this.#entries.removeOne(unique);
			this.removeUmbControllerByAlias('observeEntry_' + unique);
		}

		if (data) {
			//Check it still exists in uniques:
			const uniques = this.getUniques();
			if (!uniques.includes(unique)) {
				this.#statuses.removeOne(unique);
				return;
			}
			this.#entries.appendOne(data);

			this.#statuses.appendOne({
				state: {
					type: 'success',
				},
				unique,
			});

			if (asObservable) {
				this.observe(
					asObservable(),
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
		}
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

	override destroy(): void {
		this.#eventContext?.removeEventListener(
			UmbEntityUpdatedEvent.TYPE,
			this.#onEntityUpdatedEvent as unknown as EventListener,
		);
		super.destroy();
	}
}
