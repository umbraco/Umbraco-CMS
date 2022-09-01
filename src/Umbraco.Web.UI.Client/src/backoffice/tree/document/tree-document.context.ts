import { map } from 'rxjs';
import { UmbEntityStore } from '../../../core/stores/entity.store';
import { UmbTreeContext } from '../tree.context';

export class UmbTreeDocumentContext implements UmbTreeContext {
	public entityStore: UmbEntityStore;

	private _entityType = 'document';

	constructor(entityStore: UmbEntityStore) {
		this.entityStore = entityStore;
	}

	public fetchRoot() {
		fetch(`/umbraco/backoffice/entities?type=${this._entityType}&parentKey=`)
			.then((res) => res.json())
			.then((data) => {
				this.entityStore.update(data);
			});

		return this.entityStore.entities.pipe(
			map((items) => items.filter((item) => item.type === this._entityType && item.parentKey === '' && !item.isTrashed))
		);
	}

	public fetchChildren(key: string) {
		// TODO: figure out url structure
		fetch(`/umbraco/backoffice/entities?type=${this._entityType}&parentKey=${key}`)
			.then((res) => res.json())
			.then((data) => {
				this.entityStore.update(data);
			});

		return this.entityStore.entities.pipe(
			map((items) => items.filter((item) => item.parentKey === key && !item.isTrashed))
		);
	}
}
