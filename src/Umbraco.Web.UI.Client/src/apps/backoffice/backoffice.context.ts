import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { ServerService } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbBasicState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbExtensionsManifestInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { ManifestSection } from '@umbraco-cms/backoffice/section';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbExtensionManifestInitializer } from '@umbraco-cms/backoffice/extension-api';
import { UMB_AUTH_CONTEXT } from '@umbraco-cms/backoffice/auth';
import { UMB_CURRENT_USER_CONTEXT } from '@umbraco-cms/backoffice/current-user';
import { UmbSysinfoRepository } from '@umbraco-cms/backoffice/sysinfo';

export class UmbBackofficeContext extends UmbContextBase<UmbBackofficeContext> {
	#activeSectionAlias = new UmbStringState(undefined);
	public readonly activeSectionAlias = this.#activeSectionAlias.asObservable();

	// TODO: We need a class array state:
	readonly #allowedSections = new UmbBasicState<Array<UmbExtensionManifestInitializer<ManifestSection>>>([]);
	public readonly allowedSections = this.#allowedSections.asObservable();

	readonly #version = new UmbStringState(undefined);
	public readonly version = this.#version.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_BACKOFFICE_CONTEXT);

		// TODO: We need to ensure this request is called every time the user logs in, but this should be done somewhere across the app and not here [JOV]
		this.consumeContext(UMB_AUTH_CONTEXT, (authContext) => {
			this.observe(authContext.isAuthorized, (isAuthorized) => {
				if (!isAuthorized) return;
				this.#getVersion();
			});
		});

		this.consumeContext(UMB_CURRENT_USER_CONTEXT, (userContext) => {
			this.observe(
				userContext.allowedSections,
				(allowedSections) => {
					if (!allowedSections) return;
					// TODO: Please be aware that we re-initialize this initializer based on user permissions. I suggest we should solve this specific case should be improved by the ability to change the filter [NL]
					new UmbExtensionsManifestInitializer(
						this,
						umbExtensionsRegistry,
						'section',
						(manifest) => allowedSections.includes(manifest.alias),
						async (sections) => {
							this.#allowedSections.setValue([...sections]);
						},
						'umbAllowedSectionsManifestInitializer',
					);
				},
				'umbAllowedSectionsObserver',
			);
		});
	}

	async #getVersion() {
		const { data } = await tryExecute(ServerService.getServerInformation());
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
		const version = await this.observe(this.version).asPromise();
		const repository = new UmbSysinfoRepository(this);
		return repository.serverUpgradeCheck(version);
	}
}

export const UMB_BACKOFFICE_CONTEXT = new UmbContextToken<UmbBackofficeContext>('UmbBackofficeContext');
