import { Observable, ReplaySubject } from 'rxjs';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';
import { UmbContextToken } from '@umbraco-cms/context-api';

// TODO: maybe this should be named something else than store?
export class UmbSectionStore {
	private _currentAlias: ReplaySubject<string> = new ReplaySubject(1);
	public readonly currentAlias: Observable<string> = this._currentAlias.asObservable();

	public getAllowed() {
		// TODO: implemented allowed filtering
		/*
		const { data } = await getUserSections({});
		this._allowedSection = data.sections;
		*/

		return umbExtensionsRegistry.extensionsOfType('section');
	}

	public setCurrent(alias: string) {
		this._currentAlias.next(alias);
	}
}

export const UMB_SECTION_STORE_CONTEXT_TOKEN = new UmbContextToken<UmbSectionStore>(UmbSectionStore.name);
