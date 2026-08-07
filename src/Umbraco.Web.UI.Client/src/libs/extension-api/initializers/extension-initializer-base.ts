import type { ManifestBase } from '../types/index.js';
import type { UmbExtensionRegistry } from '../registry/extension.registry.js';
import type { SpecificManifestTypeOrManifestBase } from '../types/map.types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbElement } from '@umbraco-cms/backoffice/element-api';
import { UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';
import { combineLatest, map, type Observable } from '@umbraco-cms/backoffice/external/rxjs';

/**
 * Base class for extension initializers, which are responsible for loading and unloading extensions.
 */
export abstract class UmbExtensionInitializerBase<
	Key extends string,
	T extends ManifestBase = SpecificManifestTypeOrManifestBase<UmbExtensionManifest, Key>,
> extends UmbControllerBase {
	protected host: UmbElement;
	protected extensionRegistry: UmbExtensionRegistry<T>;
	#extensionMap = new Map();

	// Use the value `undefined`, as that would not resolve a observation promise. [NL]
	#loaded = new UmbBooleanState(undefined);
	loaded = this.#loaded.asObservable();

	// Identifies the current processing pass. The observer callback is async, so passes can
	// overlap; only the latest pass is allowed to settle `loaded`, so a slower earlier pass
	// cannot unblock waiters before the newest set of extensions has finished instantiating.
	#loadPass = 0;

	/**
	 * @param activeGate An optional observable that gates instantiation. While it emits `false` the
	 * observed set is treated as empty, so all extensions of the type unload; when it emits `true`
	 * they instantiate. Lets a subclass defer extensions until some external state is ready (e.g. an
	 * authorized user) and tear them down again when it is lost. Omit for the default always-on behaviour.
	 */
	constructor(
		host: UmbElement,
		extensionRegistry: UmbExtensionRegistry<T>,
		manifestType: Key,
		activeGate?: Observable<boolean>,
	) {
		super(host);
		this.host = host;
		this.extensionRegistry = extensionRegistry;
		const extensions$ = extensionRegistry.byType<Key, T>(manifestType);
		const source = activeGate
			? combineLatest([extensions$, activeGate]).pipe(map(([extensions, active]) => (active ? extensions : [])))
			: extensions$;
		this.observe(source, async (extensions) => {
			const pass = ++this.#loadPass;

			// Re-arm while this pass is in flight so a consumer awaiting `loaded` waits for it to
			// finish instead of resolving on a stale `true` from a previous pass. `undefined`
			// rather than `false` because `asPromise()` resolves on the first non-undefined value.
			this.#loaded.setValue(undefined);

			this.#extensionMap.forEach((existingExt) => {
				if (!extensions.find((b) => b.alias === existingExt.alias)) {
					this.unloadExtension(existingExt);
					this.#extensionMap.delete(existingExt.alias);
				}
			});

			// `allSettled` so a throwing/rejecting `instantiateExtension` cannot leave `loaded`
			// stuck at `undefined` and hang a waiter (e.g. the app boot gate). Failures are
			// surfaced rather than swallowed.
			const results = await Promise.allSettled(
				extensions.map((extension) => {
					if (this.#extensionMap.has(extension.alias)) return;
					this.#extensionMap.set(extension.alias, extension);
					return this.instantiateExtension(extension);
				}),
			);

			for (const result of results) {
				if (result.status === 'rejected') {
					console.error('[UmbExtensionInitializer] Failed to instantiate extension', result.reason);
				}
			}

			// Only the latest pass settles `loaded`. Resolving unconditionally — including for
			// zero extensions — so a consumer awaiting `loaded` (the app-entry-point boot gate,
			// the bundle guard) never hangs on a default install that registers none of this type.
			if (pass === this.#loadPass) {
				this.#loaded.setValue(true);
			}
		});
	}

	/**
	 * Perform any logic required to instantiate the extension.
	 */
	abstract instantiateExtension(manifest: T): Promise<void> | void;

	/**
	 * Perform any logic required to unload the extension.
	 */
	abstract unloadExtension(manifest: T): Promise<void> | void;
}
