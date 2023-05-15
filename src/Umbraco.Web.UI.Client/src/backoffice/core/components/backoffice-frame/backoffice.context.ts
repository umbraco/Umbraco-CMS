import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extensions-registry';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbStringState } from '@umbraco-cms/backoffice/observable-api';

export class UmbBackofficeContext {
	#activeSectionAlias = new UmbStringState(undefined);
	public readonly activeSectionAlias = this.#activeSectionAlias.asObservable();
	public readonly allowedSections = umbExtensionsRegistry.extensionsOfType('section');

	public setActiveSectionAlias(alias: string) {
		this.#activeSectionAlias.next(alias);
	}
}

export const UMB_BACKOFFICE_CONTEXT_TOKEN = new UmbContextToken<UmbBackofficeContext>('UmbBackofficeContext');
