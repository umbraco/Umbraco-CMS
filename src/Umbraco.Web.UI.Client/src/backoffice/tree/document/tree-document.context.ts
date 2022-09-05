import { map } from 'rxjs';
import { UmbTreeContextBase } from '../tree.context';

export class UmbTreeDocumentContext extends UmbTreeContextBase {
	public rootKey = 'ba23245c-d8c0-46f7-a2bc-7623743d6eba';

	public rootChanges() {
		fetch(`/umbraco/backoffice/entities/documents?parentKey=${this.rootKey}`)
			.then((res) => res.json())
			.then((data) => {
				this.entityStore.update(data);
			});

		return this.entityStore.items.pipe(
			map((items) => items.filter((item) => item.parentKey === this.rootKey && !item.isTrashed))
		);
	}

	public fetchChildren(key: string) {
		// TODO: figure out url structure
		fetch(`/umbraco/backoffice/entities/documents?parentKey=${key}`)
			.then((res) => res.json())
			.then((data) => {
				this.entityStore.update(data);
			});

		return super.childrenChanges(key);
	}
}
