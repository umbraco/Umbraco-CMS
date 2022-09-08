import { map } from 'rxjs';
import { UmbTreeDataContextBase } from '../tree-data.context';

export class UmbTreeMediaDataContext extends UmbTreeDataContextBase {
	public rootKey = 'c0858d71-52be-4bb2-822f-42fa0c9a1ea5';

	public rootChanges() {
		fetch(`/umbraco/backoffice/entities/media?parentKey=${this.rootKey}`)
			.then((res) => res.json())
			.then((data) => {
				this.entityStore.update(data);
			});

		return this.entityStore.items.pipe(map((items) => items.filter((item) => item.parentKey === this.rootKey)));
	}

	public childrenChanges(key: string) {
		// TODO: figure out url structure
		fetch(`/umbraco/backoffice/entities/media?parentKey=${key}`)
			.then((res) => res.json())
			.then((data) => {
				this.entityStore.update(data);
			});

		return super.childrenChanges(key);
	}
}
