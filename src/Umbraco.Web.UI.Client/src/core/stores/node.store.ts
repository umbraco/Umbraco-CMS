import { map, Observable } from 'rxjs';
import { NodeEntity } from '../../mocks/data/node.data';
import { UmbEntityStore } from './entity.store';
import { UmbDataStoreBase } from './store';

export class UmbNodeStore extends UmbDataStoreBase<NodeEntity> {
	private _entityStore: UmbEntityStore;

	constructor(entityStore: UmbEntityStore) {
		super();
		this._entityStore = entityStore;
	}

	getByKey(key: string): Observable<NodeEntity | null> {
		// fetch from server and update store
		fetch(`/umbraco/backoffice/node/${key}`)
			.then((res) => res.json())
			.then((data) => {
				this.update(data);
			});

		return this.items.pipe(
			map((nodes: Array<NodeEntity>) => nodes.find((node: NodeEntity) => node.key === key) || null)
		);
	}

	trash(key: string) {
		// fetch from server and update store
		// TODO: Use node type to hit the right API, or have a general Node API?
		return fetch('/umbraco/backoffice/node/trash', {
			method: 'POST',
			body: key,
			headers: {
				'Content-Type': 'application/json',
			},
		})
			.then((res) => res.json())
			.then((data: Array<NodeEntity>) => {
				this.update(data);
				this._entityStore.update(data);
			});
	}

	// TODO: make sure UI somehow can follow the status of this action.
	save(data: NodeEntity[]): Promise<void> {
		// fetch from server and update store
		// TODO: use Fetcher API.
		let body: string;

		try {
			body = JSON.stringify(data);
		} catch (error) {
			console.error(error);
			return Promise.reject();
		}

		// TODO: Use node type to hit the right API, or have a general Node API?
		return fetch('/umbraco/backoffice/node/save', {
			method: 'POST',
			body: body,
			headers: {
				'Content-Type': 'application/json',
			},
		})
			.then((res) => res.json())
			.then((data: Array<NodeEntity>) => {
				this.update(data);
				this._entityStore.update(data);
			});
	}
}
