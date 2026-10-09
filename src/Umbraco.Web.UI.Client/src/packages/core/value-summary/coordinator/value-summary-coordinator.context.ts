import type { UmbValueSummaryResolver } from '../extensions/value-summary-resolver.interface.js';
import type { ManifestValueSummary } from '../extensions/value-summary.extension.js';
import { loadValueSummaryResolver } from '../extensions/load-value-summary-resolver.function.js';
import { UMB_VALUE_SUMMARY_COORDINATOR_CONTEXT } from './value-summary-coordinator.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { debounce } from '@umbraco-cms/backoffice/utils';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
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

export class UmbValueSummaryCoordinatorContext extends UmbContextBase {
	#pending = new Map<string, Set<unknown>>();

	// TODO: Introduce a Umb Map State for better performance with large datasets and more frequent updates/lookups. [NL]
	#state = new UmbArrayState<ResolvedEntry>([], (x) => x.key);
	#resolvers = new Map<string, UmbValueSummaryResolver>();

	#scheduleFlush = debounce(() => this.#flush(), 0);

	constructor(host: UmbControllerHost) {
		super(host, UMB_VALUE_SUMMARY_COORDINATOR_CONTEXT);
	}

	/**
	 * Registers a value for display resolution.
	 * @param {string} valueType - The value type key identifying the resolver manifest.
	 * @param {unknown} value - The raw value to resolve.
	 */
	register(valueType: string, value: unknown): void {
		const manifest = this.#getManifest(valueType);

		if (!manifest?.valueResolver) {
			// No resolver — pass through raw value
			this.#state.append([{ key: `${valueType}:${toValueKey(value)}`, value }]);
		} else {
			if (!this.#pending.has(valueType)) this.#pending.set(valueType, new Set());
			this.#pending.get(valueType)!.add(value);
			this.#scheduleFlush();
		}
	}

	/**
	 * Returns an observable that emits the resolved display value for the given raw value.
	 * @param {string} valueType - The value type key identifying the resolver manifest.
	 * @param {unknown} rawValue - The raw value whose resolved form is observed.
	 * @returns {Observable<unknown | undefined>} An observable emitting the resolved value, or `undefined` until resolution completes.
	 */
	observeResolvedValue(valueType: string, rawValue: unknown): Observable<unknown | undefined> {
		const key = `${valueType}:${toValueKey(rawValue)}`;
		return this.#state.asObservablePart((items) => items.find((x) => x.key === key)?.value);
	}

	async #flush() {
		for (const [valueType, pendingValues] of this.#pending.entries()) {
			this.#pending.delete(valueType);
			const uniqueValues = [...pendingValues];
			try {
				const resolver = await this.#getOrCreateResolver(valueType);
				if (!resolver) continue;

				const { data, asObservable } = await resolver.resolveValues(uniqueValues);

				this.#applyResolved(valueType, uniqueValues, data);

				if (asObservable) {
					this.observe(
						asObservable(),
						(resolved) => this.#applyResolved(valueType, uniqueValues, resolved),
						`resolver-${valueType}`,
					);
				}
			} catch (e) {
				console.warn('[ValueSummary] Resolution failed for', valueType, e);
			}
		}
	}

	#applyResolved(valueType: string, uniqueValues: ReadonlyArray<unknown>, resolved: ReadonlyArray<unknown>) {
		const entries: ResolvedEntry[] = uniqueValues.map((v, i) => ({
			key: `${valueType}:${toValueKey(v)}`,
			value: resolved[i],
		}));
		this.#state.append(entries);
	}

	async #getOrCreateResolver(valueType: string): Promise<UmbValueSummaryResolver | undefined> {
		let resolver = this.#resolvers.get(valueType);
		if (resolver) return resolver;

		const manifest = this.#getManifest(valueType);
		if (!manifest?.valueResolver) return undefined;

		const ResolverConstructor = await loadValueSummaryResolver(manifest.valueResolver);
		if (!ResolverConstructor) return undefined;

		resolver = new ResolverConstructor(this) as UmbValueSummaryResolver;
		this.#resolvers.set(valueType, resolver);
		return resolver;
	}

	#getManifest(valueType: string): ManifestValueSummary | undefined {
		return umbExtensionsRegistry.getByTypeAndFilter(
			'valueSummary',
			(ext: ManifestValueSummary) => ext.forValueType === valueType,
		)[0];
	}

	override destroy() {
		for (const resolver of this.#resolvers.values()) {
			resolver.destroy();
		}
		this.#resolvers.clear();
		super.destroy();
	}
}
