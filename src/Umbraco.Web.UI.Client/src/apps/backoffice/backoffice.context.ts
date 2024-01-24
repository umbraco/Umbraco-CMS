import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbBasicState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import {
	type UmbExtensionManifestInitializer,
	UmbExtensionsManifestInitializer,
} from '@umbraco-cms/backoffice/extension-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { type UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { type ManifestSection, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

export class UmbBackofficeContext extends UmbContextBase<UmbBackofficeContext> {
	#activeSectionAlias = new UmbStringState(undefined);
	public readonly activeSectionAlias = this.#activeSectionAlias.asObservable();

	// TODO: We need a class array state:
	#allowedSections = new UmbBasicState<Array<UmbExtensionManifestInitializer<ManifestSection>>>([]);
	public readonly allowedSections = this.#allowedSections.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_BACKOFFICE_CONTEXT);
		new UmbExtensionsManifestInitializer(this, umbExtensionsRegistry, 'section', null, (sections) => {
			this.#allowedSections.setValue([...sections]);
		});
	}

	public setActiveSectionAlias(alias: string) {
		this.#activeSectionAlias.setValue(alias);
	}
}

export const UMB_BACKOFFICE_CONTEXT = new UmbContextToken<UmbBackofficeContext>('UmbBackofficeContext');
