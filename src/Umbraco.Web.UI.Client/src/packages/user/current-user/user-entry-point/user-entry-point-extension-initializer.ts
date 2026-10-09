import { UMB_CURRENT_USER_CONTEXT } from '../current-user.context.token.js';
import { UMB_AUTH_CONTEXT } from '@umbraco-cms/backoffice/auth';
import type { ManifestUserEntryPoint } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbElement } from '@umbraco-cms/backoffice/element-api';
import { UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';
import {
	hasInitExport,
	hasOnUnloadExport,
	loadManifestPlainJs,
	UmbExtensionInitializerBase,
	type UmbEntryPointModule,
	type UmbExtensionRegistry,
} from '@umbraco-cms/backoffice/extension-api';

/**
 * Extension initializer for the `userEntryPoint` extension type.
 *
 * Runs `onInit` once the user is authorized AND the current user data is available, so extensions
 * can rely on `UMB_CURRENT_USER_CONTEXT` being populated. Runs `onUnload` when the session ends
 * (sign-out or timeout) or the extension is unregistered. If the user authorizes again, `onInit`
 * runs again — potentially for a different user.
 */
export class UmbUserEntryPointExtensionInitializer extends UmbExtensionInitializerBase<
	'userEntryPoint',
	ManifestUserEntryPoint
> {
	readonly #active: UmbBooleanState<boolean>;
	readonly #instanceMap = new Map<string, UmbEntryPointModule>();
	#currentUserContext?: typeof UMB_CURRENT_USER_CONTEXT.TYPE;
	#isAuthorized = false;
	#hasUser = false;

	constructor(host: UmbElement, extensionRegistry: UmbExtensionRegistry<ManifestUserEntryPoint>) {
		// Create the gate before super() (no `this` access yet) and hand it to the base, which empties
		// the observed set while it is false — so extensions unload on sign-out and instantiate on sign-in.
		const active = new UmbBooleanState<boolean>(false);
		super(host, extensionRegistry, 'userEntryPoint', active.asObservable());
		this.#active = active;

		this.consumeContext(UMB_AUTH_CONTEXT, (authContext) => {
			this.observe(
				authContext?.isAuthorized,
				(isAuthorized) => {
					this.#isAuthorized = isAuthorized === true;
					if (this.#isAuthorized) {
						// Deduplicated in the context; ensures the user loads even if nothing else requests it.
						this.#currentUserContext?.load();
					}
					this.#updateActive();
				},
				'umbObserveIsAuthorized',
			);
		});

		this.consumeContext(UMB_CURRENT_USER_CONTEXT, (currentUserContext) => {
			this.#currentUserContext = currentUserContext;
			if (this.#isAuthorized) {
				currentUserContext?.load();
			}
			this.observe(
				currentUserContext?.currentUser,
				(currentUser) => {
					this.#hasUser = !!currentUser;
					this.#updateActive();
				},
				'umbObserveCurrentUser',
			);
		});
	}

	#updateActive() {
		this.#active.setValue(this.#isAuthorized && this.#hasUser);
	}

	async instantiateExtension(manifest: ManifestUserEntryPoint) {
		if (!manifest.js) return;
		const moduleInstance = await loadManifestPlainJs(manifest.js);
		// The gate may have closed (sign-out/timeout) while the module was loading; if so, the base has
		// already unloaded this alias, so skip onInit rather than run it for an ended session.
		if (!moduleInstance || !this.#active.getValue()) return;
		this.#instanceMap.set(manifest.alias, moduleInstance);
		if (hasInitExport(moduleInstance)) {
			// Promise.resolve so an async onInit is awaited even though the export type is `void`.
			await Promise.resolve(moduleInstance.onInit(this.host, this.extensionRegistry));
		}
	}

	unloadExtension(manifest: ManifestUserEntryPoint) {
		const moduleInstance = this.#instanceMap.get(manifest.alias);
		if (!moduleInstance) return;
		this.#instanceMap.delete(manifest.alias);
		if (hasOnUnloadExport(moduleInstance)) {
			// Promise.resolve also captures rejections from async onUnload exports, which a bare try/catch would miss.
			try {
				Promise.resolve(moduleInstance.onUnload(this.host, this.extensionRegistry)).catch((error) => {
					this.#logUnloadError(manifest.alias, error);
				});
			} catch (error) {
				this.#logUnloadError(manifest.alias, error);
			}
		}
	}

	#logUnloadError(alias: string, error: unknown) {
		console.error('[UmbUserEntryPointExtensionInitializer] Failed to unload extension', alias, error);
	}
}
