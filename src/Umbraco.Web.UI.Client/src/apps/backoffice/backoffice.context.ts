import { umbExtensionsRegistry } from 'src/libs/extension-registry';
import { UmbContextToken } from 'src/libs/context-api';
import { UmbStringState } from 'src/libs/observable-api';

export class UmbBackofficeContext {
	#activeSectionAlias = new UmbStringState(undefined);
	public readonly activeSectionAlias = this.#activeSectionAlias.asObservable();
	public readonly allowedSections = umbExtensionsRegistry.extensionsOfType('section');

	public setActiveSectionAlias(alias: string) {
		this.#activeSectionAlias.next(alias);
	}
}

export const UMB_BACKOFFICE_CONTEXT_TOKEN = new UmbContextToken<UmbBackofficeContext>('UmbBackofficeContext');
