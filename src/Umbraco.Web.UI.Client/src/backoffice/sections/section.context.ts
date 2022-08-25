import { BehaviorSubject } from 'rxjs';

import type { ManifestSection } from '../../core/models';

export class UmbSectionContext {
	// TODO: figure out how fine grained we want to make our observables.
	private _data = new BehaviorSubject<ManifestSection>({
		type: 'section',
		alias: '',
		name: '',
		js: '',
		elementName: '',
		meta: {
			pathname: '',
			weight: 0,
		},
	});
	public readonly data = this._data.asObservable();

	constructor(section: ManifestSection) {
		if (!section) return;
		this._data.next(section);
	}

	// TODO: figure out how we want to update data
	public update(data: Partial<ManifestSection>) {
		this._data.next({ ...this._data.getValue(), ...data });
	}

	public getData() {
		return this._data.getValue();
	}
}
