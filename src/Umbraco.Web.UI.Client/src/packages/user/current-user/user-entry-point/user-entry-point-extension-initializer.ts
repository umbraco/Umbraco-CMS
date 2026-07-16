import { UMB_CURRENT_USER_CONTEXT } from '../current-user.context.token.js';
import { UMB_AUTH_CONTEXT } from '@umbraco-cms/backoffice/auth';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbElement } from '@umbraco-cms/backoffice/element-api';
import type { ManifestUserEntryPoint } from '@umbraco-cms/backoffice/extension-registry';
import {
	hasInitExport,
	hasOnUnloadExport,
	loadManifestPlainJs,
	type UmbEntryPointModule,
	type UmbExtensionRegistry,
} from '@umbraco-cms/backoffice/extension-api';

/**
 * Extension initializer for the `userEntryPoint` extension type.
 *
 * Runs `onInit` once the user is authorized AND the current user data is available,
 * so extensions can rely on `UMB_CURRENT_USER_CONTEXT` being populated.
 * Runs `onUnload` when the session ends (sign-out or timeout) or the extension is unregistered.
 */
export class UmbUserEntryPointExtensionInitializer extends UmbControllerBase {
	#host: UmbElement;
	#extensionRegistry: UmbExtensionRegistry<ManifestUserEntryPoint>;
	#manifestMap = new Map<string, ManifestUserEntryPoint>();
	#instanceMap = new Map<string, UmbEntryPointModule>();
	#currentUserContext?: typeof UMB_CURRENT_USER_CONTEXT.TYPE;
	#isAuthorized = false;
	#hasUser = false;
	#active = false;
	// Invalidates in-flight instantiations when the session state flips while a module is loading.
	#session = 0;

	constructor(host: UmbElement, extensionRegistry: UmbExtensionRegistry<ManifestUserEntryPoint>) {
		super(host);
		this.#host = host;
		this.#extensionRegistry = extensionRegistry;

		this.consumeContext(UMB_AUTH_CONTEXT, (authContext) => {
			this.observe(
				authContext?.isAuthorized,
				(isAuthorized) => {
					this.#isAuthorized = isAuthorized === true;
					if (this.#isAuthorized) {
						// Deduplicated in the context; ensures the user loads even if nothing else requests it.
						this.#currentUserContext?.load();
					}
					this.#update();
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
					this.#update();
				},
				'umbObserveCurrentUser',
			);
		});

		this.observe(
			extensionRegistry.byType<'userEntryPoint', ManifestUserEntryPoint>(
				'userEntryPoint',
			),
			(manifests) => {
				for (const alias of [...this.#manifestMap.keys()]) {
					if (!manifests.find((manifest) => manifest.alias === alias)) {
						this.#manifestMap.delete(alias);
						this.#unloadExtension(alias);
					}
				}
				for (const manifest of manifests) {
					if (this.#manifestMap.has(manifest.alias)) continue;
					this.#manifestMap.set(manifest.alias, manifest);
					if (this.#active) {
						this.#instantiateExtension(this.#session, manifest);
					}
				}
			},
			'umbObserveUserEntryPoints',
		);
	}

	#update() {
		const active = this.#isAuthorized && this.#hasUser;
		if (active === this.#active) return;
		this.#active = active;
		const session = ++this.#session;
		if (active) {
			this.#manifestMap.forEach((manifest) => this.#instantiateExtension(session, manifest));
		} else {
			for (const alias of [...this.#instanceMap.keys()]) {
				this.#unloadExtension(alias);
			}
		}
	}

	async #instantiateExtension(session: number, manifest: ManifestUserEntryPoint) {
		if (!manifest.js) return;
		try {
			const moduleInstance = await loadManifestPlainJs(manifest.js);
			// The session may have ended, or the extension been unregistered, while the module loaded.
			if (!moduleInstance || session !== this.#session || !this.#manifestMap.has(manifest.alias)) return;
			this.#instanceMap.set(manifest.alias, moduleInstance);
			if (hasInitExport(moduleInstance)) {
				await moduleInstance.onInit(this.#host, this.#extensionRegistry);
			}
		} catch (error) {
			console.error(
				'[UmbUserEntryPointExtensionInitializer] Failed to instantiate extension',
				manifest.alias,
				error,
			);
		}
	}

	#unloadExtension(alias: string) {
		const moduleInstance = this.#instanceMap.get(alias);
		if (!moduleInstance) return;
		this.#instanceMap.delete(alias);
		if (hasOnUnloadExport(moduleInstance)) {
			// Promise.resolve also captures rejections from async onUnload exports, which a bare try/catch would miss.
			try {
				Promise.resolve(moduleInstance.onUnload(this.#host, this.#extensionRegistry)).catch((error) => {
					this.#logUnloadError(alias, error);
				});
			} catch (error) {
				this.#logUnloadError(alias, error);
			}
		}
	}

	#logUnloadError(alias: string, error: unknown) {
		console.error('[UmbUserEntryPointExtensionInitializer] Failed to unload extension', alias, error);
	}

	override destroy() {
		for (const alias of [...this.#instanceMap.keys()]) {
			this.#unloadExtension(alias);
		}
		super.destroy();
	}
}
