import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { ServerService } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbBasicState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbExtensionsManifestInitializer, UmbServerExtensionRegistrator } from '@umbraco-cms/backoffice/extension-api';
import { UmbSysinfoRepository } from '@umbraco-cms/backoffice/sysinfo';
import { UMB_AUTH_CONTEXT } from '@umbraco-cms/backoffice/auth';
import { UMB_CURRENT_USER_CONTEXT } from '@umbraco-cms/backoffice/current-user';
import type { ManifestSection } from '@umbraco-cms/backoffice/section';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type {
	ManifestBase,
	UmbBundleExtensionInitializer,
	UmbExtensionManifestInitializer,
} from '@umbraco-cms/backoffice/extension-api';

const CORE_PACKAGES: Array<Promise<{ name: string; extensions: Array<any> }>> = [
	import('../../packages/block/umbraco-package.js'),
	import('../../packages/clipboard/umbraco-package.js'),
	import('../../packages/code-editor/umbraco-package.js'),
	import('../../packages/content/umbraco-package.js'),
	import('../../packages/data-type/umbraco-package.js'),
	import('../../packages/dictionary/umbraco-package.js'),
	import('../../packages/documents/umbraco-package.js'),
	import('../../packages/embedded-media/umbraco-package.js'),
	import('../../packages/extension-insights/umbraco-package.js'),
	import('../../packages/health-check/umbraco-package.js'),
	import('../../packages/help/umbraco-package.js'),
	import('../../packages/language/umbraco-package.js'),
	import('../../packages/log-viewer/umbraco-package.js'),
	import('../../packages/management-api/umbraco-package.js'),
	import('../../packages/markdown-editor/umbraco-package.js'),
	import('../../packages/media/umbraco-package.js'),
	import('../../packages/members/umbraco-package.js'),
	import('../../packages/models-builder/umbraco-package.js'),
	import('../../packages/multi-url-picker/umbraco-package.js'),
	import('../../packages/packages/umbraco-package.js'),
	import('../../packages/performance-profiling/umbraco-package.js'),
	import('../../packages/property-editors/umbraco-package.js'),
	import('../../packages/publish-cache/umbraco-package.js'),
	import('../../packages/relations/umbraco-package.js'),
	import('../../packages/rte/umbraco-package.js'),
	import('../../packages/settings/umbraco-package.js'),
	import('../../packages/static-file/umbraco-package.js'),
	import('../../packages/sysinfo/umbraco-package.js'),
	import('../../packages/tags/umbraco-package.js'),
	import('../../packages/telemetry/umbraco-package.js'),
	import('../../packages/templating/umbraco-package.js'),
	import('../../packages/tiptap/umbraco-package.js'),
	import('../../packages/translation/umbraco-package.js'),
	import('../../packages/ufm/umbraco-package.js'),
	import('../../packages/umbraco-news/umbraco-package.js'),
	import('../../packages/user/umbraco-package.js'),
	import('../../packages/webhook/umbraco-package.js'),
];
export class UmbBackofficeContext extends UmbContextBase {
	#currentUser?: typeof UMB_CURRENT_USER_CONTEXT.TYPE;

	#activeSectionAlias = new UmbStringState(undefined);
	public readonly activeSectionAlias = this.#activeSectionAlias.asObservable();

	// TODO: We need a class array state:
	readonly #allowedSections = new UmbBasicState<Array<UmbExtensionManifestInitializer<ManifestSection>>>([]);
	public readonly allowedSections = this.#allowedSections.asObservable();

	readonly #version = new UmbStringState(undefined);
	public readonly version = this.#version.asObservable();

	#packageModules?: Promise<Array<{ name: string; extensions: Array<ManifestBase> }>>;

	constructor(host: UmbControllerHost, bundleInitializer?: UmbBundleExtensionInitializer) {
		super(host, UMB_BACKOFFICE_CONTEXT);

		// TODO: We need to ensure this request is called every time the user logs in, but this should be done somewhere across the app and not here [JOV]
		this.consumeContext(UMB_AUTH_CONTEXT, (authContext) => {
			this.observe(
				authContext?.isAuthorized,
				async (isAuthorized) => {
					if (isAuthorized === undefined) return;
					if (isAuthorized) {
						await Promise.all([
							this.#registerExtensions(),
							this.#getVersion(),
							new UmbServerExtensionRegistrator(this, umbExtensionsRegistry).registerPrivateExtensions(),
						]);

						if (bundleInitializer) {
							// Await all bundles got loaded?
							await this.observe(bundleInitializer?.loaded).asPromise();
						}

						this.#loadCurrentUser();
					} else {
						// TODO: Unregistering all extensions from v.18 [NL]
						//void this.#unregisterExtensions();
					}
				},
				'observeIsAuthorized',
			);
		});

		this.consumeContext(UMB_CURRENT_USER_CONTEXT, (userContext) => {
			this.#currentUser = userContext;
			this.#loadCurrentUser();
			this.observe(
				userContext?.allowedSections,
				(allowedSections) => {
					if (!allowedSections) return;
					// TODO: Please be aware that we re-initialize this initializer based on user permissions. I suggest we should solve this specific case should be improved by the ability to change the filter [NL]
					new UmbExtensionsManifestInitializer(
						this,
						umbExtensionsRegistry,
						'section',
						(manifest) => allowedSections.includes(manifest.alias),
						async (sections) => {
							this.#allowedSections.setValue(sections);
						},
						'umbAllowedSectionsManifestInitializer',
					);
				},
				'umbAllowedSectionsObserver',
			);
		});
	}

	async #registerExtensions() {
		if (this.#packageModules === undefined) {
			this.#packageModules = Promise.all(CORE_PACKAGES);
		}

		umbExtensionsRegistry.registerMany((await this.#packageModules).flatMap((modules) => modules.extensions));
	}

	/*
	async #unregisterExtensions() {
		if (!this.#packageModules) return;
		(await this.#packageModules).forEach((packageModule) => {
			const aliases = packageModule.extensions.map((extension) => extension.alias);
			umbExtensionsRegistry.unregisterMany(aliases);
		});
	}
		*/

	#loadCurrentUser() {
		if (!this.#currentUser || !this.#packageModules) return;
		this.#currentUser.load();
	}

	async #getVersion() {
		const { data } = await tryExecute(this._host, ServerService.getServerInformation(), { disableNotifications: true });
		if (!data) return;

		// A quick semver parser (to remove the unwanted bits) [LK]
		// https://semver.org/#is-there-a-suggested-regular-expression-regex-to-check-a-semver-string
		// eslint-disable-next-line @typescript-eslint/no-unused-vars
		const [semVer, major, minor, patch, prerelease, buildmetadata] =
			data.version.match(
				/^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)(?:-((?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+([0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$/,
			) ?? [];

		const version = [major, minor, patch].join('.') + (prerelease ? `-${prerelease}` : '');
		this.#version.setValue(version);
	}

	public setActiveSectionAlias(alias: string) {
		this.#activeSectionAlias.setValue(alias);
	}

	public async serverUpgradeCheck() {
		const version = await this.observe(this.version)
			.asPromise()
			.catch(() => null);
		if (!version) return null;
		const repository = new UmbSysinfoRepository(this);
		return repository.serverUpgradeCheck(version);
	}
}

export const UMB_BACKOFFICE_CONTEXT = new UmbContextToken<UmbBackofficeContext>('UmbBackofficeContext');
