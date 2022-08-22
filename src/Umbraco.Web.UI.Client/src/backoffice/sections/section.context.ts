import { BehaviorSubject, Observable } from 'rxjs';
import { UmbExtensionManifestSection } from '../../core/extension';

export class UmbSectionContext {
	// TODO: figure out how fine grained we want to make our observables.
	private _data: BehaviorSubject<UmbExtensionManifestSection> = new BehaviorSubject({
		type: 'section',
		alias: '',
		name: '',
		meta: {
			pathname: '',
			weight: 0,
		},
	});
	public readonly data: Observable<UmbExtensionManifestSection> = this._data.asObservable();

	constructor(section: UmbExtensionManifestSection) {
		if (!section) return;
		this._data.next(section);
	}

	// TODO: figure out how we want to update data
	public update(data: Partial<UmbExtensionManifestSection>) {
		this._data.next({ ...this._data.getValue(), ...data });
	}

	public getData() {
		return this._data.getValue();
	}
}
