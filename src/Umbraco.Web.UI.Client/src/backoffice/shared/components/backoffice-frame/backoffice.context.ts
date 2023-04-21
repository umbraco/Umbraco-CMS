import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extensions-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbStringState } from '@umbraco-cms/backoffice/observable-api';

export class UmbBackofficeContext {
	#activeSectionAlias = new UmbStringState(undefined);
	public readonly activeSectionAlias = this.#activeSectionAlias.asObservable();

	public getAllowedSections() {
		// TODO: implemented allowed filtering based on user, maybe this will be a general need and solved else where so this might not be needed in the end.
		/*
		const { data } = await getUserSections({});
		this._allowedSection = data.sections;
		*/
		return umbExtensionsRegistry.extensionsOfType('section');
	}

	public setActiveSectionAlias(alias: string) {
		this.#activeSectionAlias.next(alias);
	}
}

export const UMB_BACKOFFICE_CONTEXT_TOKEN = new UmbContextToken<UmbBackofficeContext>('UmbBackofficeContext');
