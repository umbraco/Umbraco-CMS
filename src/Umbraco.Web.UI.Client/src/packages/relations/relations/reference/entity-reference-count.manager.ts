import type { UmbEntityReferenceRepository } from './types.js';
import { UmbBasicState } from '@umbraco-cms/backoffice/observable-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export interface UmbEntityReferenceCountManagerArgs {
	referenceRepositoryAlias: string;
}

/**
 * Tracks the total number of items referencing a given entity, so a consumer (e.g. a publishing workspace context)
 * can make a decision that depends on the count (like whether to show a confirmation dialog) without awaiting a
 * network round-trip at the point of decision.
 * @exports
 * @class UmbEntityReferenceCountManager
 * @augments {UmbControllerBase}
 */
export class UmbEntityReferenceCountManager extends UmbControllerBase {
	#total = new UmbBasicState<number | undefined>(undefined);
	/** Observable emitting the current reference count. `undefined` until the first successful load. */
	public readonly total = this.#total.asObservable();

	readonly #referenceRepositoryAlias: string;
	#unique?: string;
	#reloadToken = 0;
	#repository?: UmbEntityReferenceRepository;
	#repositoryPromise?: Promise<UmbEntityReferenceRepository>;

	constructor(host: UmbControllerHost, args: UmbEntityReferenceCountManagerArgs) {
		super(host);
		this.#referenceRepositoryAlias = args.referenceRepositoryAlias;
	}

	/**
	 * Get the current reference count, if known. `undefined` means it hasn't loaded yet — prefer
	 * {@link getTotalAsync} when the caller needs a definite answer.
	 * @returns {number | undefined}
	 */
	getTotal(): number | undefined {
		return this.#total.getValue();
	}

	/**
	 * Get the current reference count, loading it first if it hasn't loaded yet. Use this at decision points
	 * (e.g. "should a confirmation dialog open?") so an unlucky race with the initial load can't silently read as 0.
	 * @returns {Promise<number>}
	 */
	async getTotalAsync(): Promise<number> {
		const current = this.getTotal();
		if (current !== undefined) return current;
		await this.reload();
		return this.getTotal() ?? 0;
	}

	/**
	 * Set the entity to count references for, and (re)load the count. A no-op if the unique is unchanged.
	 * @param {string | undefined} unique - The unique identifier of the entity, or `undefined` to clear.
	 * @returns {Promise<void>}
	 */
	async setUnique(unique: string | undefined): Promise<void> {
		if (this.#unique === unique) return;
		this.#unique = unique;
		await this.reload();
	}

	/**
	 * Reload the reference count for the current unique.
	 * @returns {Promise<void>}
	 */
	async reload(): Promise<void> {
		const unique = this.#unique;
		const token = ++this.#reloadToken;

		if (!unique) {
			this.#total.setValue(undefined);
			return;
		}

		const repository = await this.#getRepository();
		const { data } = await repository.requestReferencedBy(unique, 0, 1);

		// A newer reload (e.g. the unique changed again) has since started — its result should win, not ours.
		if (token !== this.#reloadToken) return;

		this.#total.setValue(data?.total ?? 0);
	}

	async #getRepository(): Promise<UmbEntityReferenceRepository> {
		if (this.#repository) return this.#repository;
		this.#repositoryPromise ??= createExtensionApiByAlias<UmbEntityReferenceRepository>(
			this,
			this.#referenceRepositoryAlias,
		);
		this.#repository = await this.#repositoryPromise;
		return this.#repository;
	}
}
