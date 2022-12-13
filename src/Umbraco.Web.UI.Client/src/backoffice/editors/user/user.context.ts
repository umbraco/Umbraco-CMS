import { BehaviorSubject, Observable } from 'rxjs';
import type { UserDetails } from '@umbraco-cms/models';

export class UmbUserContext {
	// TODO: figure out how fine grained we want to make our observables.
	private _data = new BehaviorSubject<UserDetails>({
		key: '',
		name: '',
		icon: '',
		type: 'user',
		hasChildren: false,
		parentKey: '',
		email: '',
		language: '',
		status: 'enabled',
		updateDate: '8/27/2022',
		createDate: '9/19/2022',
		failedLoginAttempts: 0,
		userGroups: [],
		contentStartNodes: [],
		mediaStartNodes: [],
	});
	public readonly data: Observable<UserDetails> = this._data.asObservable();

	constructor(user: UserDetails) {
		if (!user) return;
		this._data.next(user);
	}

	// TODO: figure out how we want to update data
	public update(data: Partial<UserDetails>) {
		this._data.next({ ...this._data.getValue(), ...data });
	}

	public getData() {
		return this._data.getValue();
	}
}
