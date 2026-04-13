import type { UmbValueSummaryResolver } from '../extensions/value-summary-resolver.interface.js';
import type { ManifestValueSummary } from '../extensions/value-summary.extension.js';
import { UMB_VALUE_SUMMARY_COORDINATOR_CONTEXT } from './value-summary-coordinator.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { debounce } from '@umbraco-cms/backoffice/utils';
import { loadManifestApi } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';

interface ResolvedEntry {
	key: string;
	value: unknown;
}

/**
 * Converts a value to a stable string key for caching.
 */
function toValueKey(v: unknown): string {
	return v !== null && typeof v === 'object' ? JSON.stringify(v) : String(v);
}

export class UmbValueSummaryCoordinatorContext extends UmbContextBase {
	#pending = new Map<string, Set<unknown>>();
	#state = new UmbArrayState<ResolvedEntry>([], (x) => x.key);
	#resolvers = new Map<string, UmbValueSummaryResolver>();

	#scheduleFlush = debounce(() => this.#flush(), 0);

	constructor(host: UmbControllerHost) {
		super(host, UMB_VALUE_SUMMARY_COORDINATOR_CONTEXT);
	}

	preRegister(valueType: string, values: ReadonlyArray<unknown>): void {
		const manifest = this.#getManifest(valueType);

		if (!manifest?.resolver) {
			// No resolver — pass through raw values
			this.#state.append(values.map((v) => ({ key: `${valueType}:${toValueKey(v)}`, value: v })));
		} else {
			if (!this.#pending.has(valueType)) this.#pending.set(valueType, new Set());
			const pendingSet = this.#pending.get(valueType)!;
			for (const v of values) pendingSet.add(v);
			this.#scheduleFlush();
		}
	}

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

				const resolved = await resolver.resolveValues(uniqueValues);
				const entries: ResolvedEntry[] = uniqueValues.map((v, i) => ({
					key: `${valueType}:${toValueKey(v)}`,
					value: resolved[i],
				}));
				this.#state.append(entries);
			} catch (e) {
				console.warn('[ValueSummary] Resolution failed for', valueType, e);
			}
		}
	}

	async #getOrCreateResolver(valueType: string): Promise<UmbValueSummaryResolver | undefined> {
		let resolver = this.#resolvers.get(valueType);
		if (resolver) return resolver;

		const manifest = this.#getManifest(valueType);
		if (!manifest?.resolver) return undefined;

		const ResolverConstructor = await loadManifestApi(manifest.resolver);
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
