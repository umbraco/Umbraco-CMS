import { BehaviorSubject, Observable } from 'rxjs';
import type { UserGroupDetails } from '@umbraco-cms/models';

export class UmbUserGroupContext {
	// TODO: figure out how fine grained we want to make our observables.
	private _data = new BehaviorSubject<UserGroupDetails>({
		key: '',
		name: '',
		icon: '',
		type: 'userGroup',
		hasChildren: false,
		parentKey: '',
		isTrashed: false,
		sections: [],
	});
	public readonly data: Observable<UserGroupDetails> = this._data.asObservable();

	constructor(userGroup: UserGroupDetails) {
		if (!userGroup) return;
		this._data.next(userGroup);
	}

	// TODO: figure out how we want to update data
	public update(data: Partial<UserGroupDetails>) {
		this._data.next({ ...this._data.getValue(), ...data });
	}

	public getData() {
		return this._data.getValue();
	}
}
