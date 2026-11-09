import type { UmbDisplayBatchResolver, UmbDisplayBatchResolverLookup } from './display-batch-resolver.interface.js';
import { loadDisplayBatchResolver } from './load-display-batch-resolver.function.js';
import type { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { debounce } from '@umbraco-cms/backoffice/utils';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';

interface ResolvedEntry {
	key: string;
	value: unknown;
}

/**
 * Converts a value to a stable string key for caching.
 * @param {unknown} v value
 * @returns {string} key
 */
function toValueKey(v: unknown): string {
	return v !== null && typeof v === 'object' ? JSON.stringify(v) : `${typeof v}:${String(v)}`;
}

/**
 * A context that resolves the values its descendant elements display. Elements register values
 * independently; registrations within the same tick are coalesced into one call per resolver, and
 * each resolved value is observable on its own.
 *
 * Each owner runs its own instance with its own context token and lookup, so keys only need to be
 * unique within that owner. Resolved values are kept for the lifetime of the coordinator.
 */
export class UmbDisplayBatchResolverCoordinatorContext extends UmbContextBase {
	#lookup: UmbDisplayBatchResolverLookup;
	#pending = new Map<string, Set<unknown>>();

	// TODO: Introduce a Umb Map State for better performance with large datasets and more frequent updates/lookups. [NL]
	#state = new UmbArrayState<ResolvedEntry>([], (x) => x.key);
	#resolvers = new Map<string, UmbDisplayBatchResolver>();

	#scheduleFlush = debounce(() => this.#flush(), 0);

	constructor(
		host: UmbControllerHost,
		contextToken: UmbContextToken<UmbDisplayBatchResolverCoordinatorContext>,
		lookup: UmbDisplayBatchResolverLookup,
	) {
		super(host, contextToken);
		this.#lookup = lookup;
	}

	/**
	 * Registers a value for resolution.
	 * Values of a key without a resolver are passed through unchanged.
	 * @param {string} key - The key identifying the resolver.
	 * @param {unknown} value - The raw value to resolve.
	 */
	register(key: string, value: unknown): void {
		if (!this.#lookup(key)) {
			this.#state.append([{ key: `${key}:${toValueKey(value)}`, value }]);
		} else {
			if (!this.#pending.has(key)) this.#pending.set(key, new Set());
			this.#pending.get(key)!.add(value);
			this.#scheduleFlush();
		}
	}

	/**
	 * Returns an observable that emits the resolved value for the given raw value.
	 * @param {string} key - The key identifying the resolver.
	 * @param {unknown} rawValue - The raw value whose resolved form is observed.
	 * @returns {Observable<unknown | undefined>} An observable emitting the resolved value, or `undefined` until resolution completes.
	 */
	observeResolvedValue(key: string, rawValue: unknown): Observable<unknown | undefined> {
		const entryKey = `${key}:${toValueKey(rawValue)}`;
		return this.#state.asObservablePart((items) => items.find((x) => x.key === entryKey)?.value);
	}

	async #flush() {
		for (const [key, pendingValues] of this.#pending.entries()) {
			this.#pending.delete(key);
			const uniqueValues = [...pendingValues];
			try {
				const resolver = await this.#getOrCreateResolver(key);
				if (!resolver) continue;

				const { data, asObservable } = await resolver.resolveValues(uniqueValues);

				this.#applyResolved(key, uniqueValues, data);

				if (asObservable) {
					// Each flush covers different values, so its observer must not replace an earlier flush's observer.
					this.observe(asObservable(), (resolved) => this.#applyResolved(key, uniqueValues, resolved), null);
				}
			} catch (e) {
				console.warn('[DisplayBatchResolver] Resolution failed for', key, e);
			}
		}
	}

	#applyResolved(key: string, uniqueValues: ReadonlyArray<unknown>, resolved: ReadonlyArray<unknown>) {
		const entries: ResolvedEntry[] = uniqueValues.map((v, i) => ({
			key: `${key}:${toValueKey(v)}`,
			value: resolved[i],
		}));
		this.#state.append(entries);
	}

	async #getOrCreateResolver(key: string): Promise<UmbDisplayBatchResolver | undefined> {
		let resolver = this.#resolvers.get(key);
		if (resolver) return resolver;

		const property = this.#lookup(key);
		if (!property) return undefined;

		const ResolverConstructor = await loadDisplayBatchResolver(property);
		if (!ResolverConstructor) return undefined;

		resolver = new ResolverConstructor(this) as UmbDisplayBatchResolver;
		this.#resolvers.set(key, resolver);
		return resolver;
	}

	override destroy() {
		for (const resolver of this.#resolvers.values()) {
			resolver.destroy();
		}
		this.#resolvers.clear();
		super.destroy();
	}
}
