import { map } from 'rxjs';
import { UmbTreeContextBase } from '../tree.context';

export class UmbTreeMediaContext extends UmbTreeContextBase {
	private _rootKey = 'c0858d71-52be-4bb2-822f-42fa0c9a1ea5';

	public fetchRoot() {
		fetch(`/umbraco/backoffice/entities/media?parentKey=${this._rootKey}`)
			.then((res) => res.json())
			.then((data) => {
				this.entityStore.update(data);
			});

		return this.entityStore.entities.pipe(map((items) => items.filter((item) => item.parentKey === this._rootKey)));
	}

	public fetchChildren(key: string) {
		// TODO: figure out url structure
		fetch(`/umbraco/backoffice/entities/media?parentKey=${key}`)
			.then((res) => res.json())
			.then((data) => {
				this.entityStore.update(data);
			});

		return this.entityStore.entities.pipe(map((items) => items.filter((item) => item.parentKey === key)));
	}
}
