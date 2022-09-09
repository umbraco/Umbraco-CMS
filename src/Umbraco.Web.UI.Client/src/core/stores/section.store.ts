import { map, Observable, ReplaySubject } from 'rxjs';
import { UmbExtensionRegistry } from '../extension';

// TODO: maybe this should be named something else than store?
export class UmbSectionStore {
	private _extensionRegistry: UmbExtensionRegistry;

	private _currentAlias: ReplaySubject<string> = new ReplaySubject(1);
	public readonly currentAlias: Observable<string> = this._currentAlias.asObservable();

	// TODO: how do we want to handle DI in contexts?
	constructor(extensionRegistry: UmbExtensionRegistry) {
		this._extensionRegistry = extensionRegistry;
	}

	public getAllowed() {
		// TODO: implemented allowed filtering
		/*
		const { data } = await getUserSections({});
		this._allowedSection = data.sections;
		*/

		return this._extensionRegistry
			?.extensionsOfType('section')
			.pipe(map((extensions) => extensions.sort((a, b) => b.meta.weight - a.meta.weight)));
	}

	public setCurrent(alias: string) {
		this._currentAlias.next(alias);
	}
}
