import type { UmbValueSummaryApi } from '../extensions/value-summary-api.interface.js';
import type { ManifestValueSummary } from '../extensions/value-summary.extension.js';
import { UMB_VALUE_SUMMARY_COORDINATOR_CONTEXT } from './value-summary-coordinator.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { debounce } from '@umbraco-cms/backoffice/utils';
import { createExtensionApi, type ManifestApi } from '@umbraco-cms/backoffice/extension-api';
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

	#scheduleFlush = debounce(() => this.#flush(), 0);

	constructor(host: UmbControllerHost) {
		super(host, UMB_VALUE_SUMMARY_COORDINATOR_CONTEXT);
	}

	preRegister(valueType: string, values: ReadonlyArray<unknown>): void {
		const manifests = umbExtensionsRegistry.getByTypeAndFilter(
			'valueSummary',
			(ext: ManifestValueSummary) => ext.forValueType === valueType,
		);
		const manifest = manifests[0];

		if (!manifest?.api) {
			// No API — pass through raw values
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
				const manifests = umbExtensionsRegistry.getByTypeAndFilter(
					'valueSummary',
					(ext: ManifestValueSummary) => ext.forValueType === valueType,
				);
				const manifest = manifests[0];
				if (!manifest?.api) continue;

				const api = await createExtensionApi<UmbValueSummaryApi>(this, manifest as unknown as ManifestApi<UmbValueSummaryApi>);
				if (!api) continue;

				const resolved = await api.resolveValues(uniqueValues);
				const entries: ResolvedEntry[] = uniqueValues.map((v, i) => ({
					key: `${valueType}:${toValueKey(v)}`,
					value: resolved[i],
				}));
				this.#state.append(entries);
				api.destroy();
			} catch (e) {
				console.warn('[ValueSummary] Resolution failed for', valueType, e);
			}
		}
	}
}
