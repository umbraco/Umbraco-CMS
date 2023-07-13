import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbBasicState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import { UmbManifestExtensionController, UmbManifestsExtensionController } from '@umbraco-cms/backoffice/extension-api';
import { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { ManifestSection } from '@umbraco-cms/backoffice/extension-registry';

export class UmbBackofficeContext {
	#activeSectionAlias = new UmbStringState(undefined);
	public readonly activeSectionAlias = this.#activeSectionAlias.asObservable();

	// TODO: We need a class array state:
	#allowedSections = new UmbBasicState<Array<UmbManifestExtensionController<ManifestSection>>>([]);
	public readonly allowedSections = this.#allowedSections.asObservable();

	constructor(host: UmbControllerHost) {
		new UmbManifestsExtensionController<ManifestSection>(host, 'section', null, (sections) => {
			console.log('allowed sections', sections);
			this.#allowedSections.next([...sections]);
		});
	}

	public setActiveSectionAlias(alias: string) {
		this.#activeSectionAlias.next(alias);
	}
}

export const UMB_BACKOFFICE_CONTEXT_TOKEN = new UmbContextToken<UmbBackofficeContext>('UmbBackofficeContext');
