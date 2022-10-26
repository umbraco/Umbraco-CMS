import { Observable, ReplaySubject } from 'rxjs';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';

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
