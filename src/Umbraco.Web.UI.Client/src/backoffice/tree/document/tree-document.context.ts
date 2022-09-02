import { map } from 'rxjs';
import { UmbEntityStore } from '../../../core/stores/entity.store';
import { UmbTreeContext } from '../tree.context';

export class UmbTreeDocumentContext implements UmbTreeContext {
	public entityStore: UmbEntityStore;

	private _rootKey = 'ba23245c-d8c0-46f7-a2bc-7623743d6eba';

	constructor(entityStore: UmbEntityStore) {
		this.entityStore = entityStore;
	}

	public fetchRoot() {
		fetch(`/umbraco/backoffice/entities/documents?parentKey=${this._rootKey}`)
			.then((res) => res.json())
			.then((data) => {
				this.entityStore.update(data);
			});

		return this.entityStore.entities.pipe(
			map((items) => items.filter((item) => item.parentKey === this._rootKey && !item.isTrashed))
		);
	}

	public fetchChildren(key: string) {
		// TODO: figure out url structure
		fetch(`/umbraco/backoffice/entities/documents?parentKey=${key}`)
			.then((res) => res.json())
			.then((data) => {
				this.entityStore.update(data);
			});

		return this.entityStore.entities.pipe(
			map((items) => items.filter((item) => item.parentKey === key && !item.isTrashed))
		);
	}
}
