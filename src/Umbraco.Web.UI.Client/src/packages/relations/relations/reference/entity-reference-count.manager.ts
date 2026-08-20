import type { UmbEntityReferenceRepository } from './types.js';
import { UmbBasicState } from '@umbraco-cms/backoffice/observable-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export interface UmbEntityReferenceCountManagerArgs {
	referenceRepositoryAlias: string;
	/**
	 * When `false`, `setUnique` only resets the cached total rather than loading it — the first
	 * `getTotalAsync()` call after that loads it on demand. Defaults to `true`.
	 */
	prefetch?: boolean;
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
	readonly #total = new UmbBasicState<number | undefined>(undefined);
	/** Observable emitting the current reference count. `undefined` until the first successful load. */
	public readonly total = this.#total.asObservable();

	readonly #referenceRepositoryAlias: string;
	readonly #prefetch: boolean;
	#unique?: string;
	#reloadToken = 0;
	#repository?: UmbEntityReferenceRepository;
	#repositoryPromise?: Promise<UmbEntityReferenceRepository>;
	#pendingReload?: Promise<void>;

	constructor(host: UmbControllerHost, args: UmbEntityReferenceCountManagerArgs) {
		super(host);
		this.#referenceRepositoryAlias = args.referenceRepositoryAlias;
		this.#prefetch = args.prefetch ?? true;
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
	 * Piggybacks on a reload already in flight (e.g. from {@link setUnique}) rather than starting a second,
	 * redundant one — starting a second one here would race the first and could win with a stale/empty result.
	 * @returns {Promise<number>} The reference count.
	 */
	async getTotalAsync(): Promise<number> {
		const current = this.getTotal();
		if (current !== undefined) return current;
		await (this.#pendingReload ?? this.reload());
		return this.getTotal() ?? 0;
	}

	/**
	 * Set the entity to count references for. A no-op if the unique is unchanged. When `prefetch` is enabled
	 * (the default), also (re)loads the count immediately; otherwise the count is loaded lazily, on the first
	 * {@link getTotalAsync} call.
	 * @param {string | undefined} unique - The unique identifier of the entity, or `undefined` to clear.
	 * @returns {Promise<void>}
	 */
	async setUnique(unique: string | undefined): Promise<void> {
		if (this.#unique === unique) return;
		this.#unique = unique;

		if (!this.#prefetch) {
			this.#invalidate();
			return;
		}

		await this.reload();
	}

	/**
	 * Drops the cached count (if any) without changing the current unique, so the next {@link getTotalAsync} call
	 * re-reads it from the server. Cheaper than {@link reload} for lazy (`prefetch: false`) consumers that don't
	 * need the fresh value immediately — e.g. right after a publish/unpublish/schedule action.
	 * @returns {void}
	 */
	clear(): void {
		this.#invalidate();
	}

	/**
	 * Invalidate any reload already in flight and drop the cached total, without touching {@link #unique}.
	 * Shared by {@link clear} and the lazy branch of {@link setUnique} so the two can't drift apart — leaving
	 * `#pendingReload` set here would let a subsequent {@link getTotalAsync} call ride along on a reload for a
	 * unique this manager no longer cares about, and read its (irrelevant) result as the answer.
	 * @returns {void}
	 */
	#invalidate(): void {
		++this.#reloadToken;
		this.#pendingReload = undefined;
		this.#total.setValue(undefined);
	}

	/**
	 * Reload the reference count for the current unique.
	 * @returns {Promise<void>}
	 */
	async reload(): Promise<void> {
		const promise = this.#doReload();
		this.#pendingReload = promise;
		try {
			await promise;
		} finally {
			if (this.#pendingReload === promise) this.#pendingReload = undefined;
		}
	}

	async #doReload(): Promise<void> {
		const unique = this.#unique;
		const token = ++this.#reloadToken;

		if (!unique) {
			this.#total.setValue(undefined);
			return;
		}

		const total = await this.#requestTotal(unique);

		// A newer reload (e.g. the unique changed again) has since started — its result should win, not ours.
		if (token !== this.#reloadToken) return;

		this.#total.setValue(total);
	}

	async #requestTotal(unique: string): Promise<number> {
		const repository = await this.#getRepository();

		const { data, error } = await repository.requestReferencedBy(unique, 0, 1);
		if (error) throw error;
		return data?.total ?? 0;
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
