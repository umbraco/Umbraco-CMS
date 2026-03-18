import { UMB_VALUE_MINIMAL_DISPLAY_COORDINATOR_CONTEXT } from './value-minimal-display-coordinator.context-token.js';
import type { UmbValueMinimalDisplayApi } from '../extensions/value-minimal-display-api.interface.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { debounce } from '@umbraco-cms/backoffice/utils';
import { umbExtensionsRegistry, createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';

interface ResolvedEntry {
	key: string;
	value: unknown;
}

function toValueKey(v: unknown): string {
	return v !== null && typeof v === 'object' ? JSON.stringify(v) : String(v);
}

export class UmbValueMinimalDisplayCoordinatorContext extends UmbContextBase {
	#pending = new Map<string, Set<unknown>>();
	#state = new UmbArrayState<ResolvedEntry>([], (x) => x.key);

	#scheduleFlush = debounce(() => this.#flush(), 0);

	constructor(host: UmbControllerHost) {
		super(host, UMB_VALUE_MINIMAL_DISPLAY_COORDINATOR_CONTEXT);
	}

	preRegister(alias: string, values: ReadonlyArray<unknown>): void {
		const manifest = umbExtensionsRegistry.getByAlias(alias) as { api?: unknown } | undefined;
		if (!manifest?.api) {
			this.#state.append(
				values.map((v) => ({ key: `${alias}:${toValueKey(v)}`, value: v })),
			);
		} else {
			if (!this.#pending.has(alias)) this.#pending.set(alias, new Set());
			const pendingSet = this.#pending.get(alias)!;
			for (const v of values) pendingSet.add(v);
			this.#scheduleFlush();
		}
	}

	observeResolvedValue(alias: string, rawValue: unknown): Observable<unknown | undefined> {
		const key = `${alias}:${toValueKey(rawValue)}`;
		return this.#state.asObservablePart((items) => items.find((x) => x.key === key)?.value);
	}

	async #flush() {
		for (const [alias, pendingValues] of this.#pending.entries()) {
			this.#pending.delete(alias);
			const uniqueValues = [...pendingValues];
			try {
				const api = await createExtensionApiByAlias<UmbValueMinimalDisplayApi>(this, alias);
				const resolved = await api.resolveValues(uniqueValues);
				const entries: ResolvedEntry[] = uniqueValues.map((v) => ({
					key: `${alias}:${toValueKey(v)}`,
					value: resolved.get(toValueKey(v)),
				}));
				this.#state.append(entries);
				api.destroy();
			} catch {
				// Silently skip if API creation or resolution fails
			}
		}
	}
}
