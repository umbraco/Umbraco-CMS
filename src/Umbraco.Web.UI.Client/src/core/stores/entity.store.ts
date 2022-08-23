import { BehaviorSubject, map, Observable } from 'rxjs';
import { Entity } from '../../mocks/data/entity.data';
import { umbNodeData } from '../../mocks/data/node.data';

export class UmbEntityStore {
	private _entities: BehaviorSubject<Array<Entity>> = new BehaviorSubject(<Array<Entity>>[]);
	public readonly entities: Observable<Array<Entity>> = this._entities.asObservable();

	getById(id: number): Observable<Entity | null> {
		// fetch from server and update store
		fetch(`/umbraco/backoffice/content/${id}`)
			.then((res) => res.json())
			.then((data) => {
				this._updateStore(data);
			});

		return this.entities.pipe(map((nodes: Array<Entity>) => nodes.find((node: Entity) => node.id === id) || null));
	}

	private _updateStore(fetchedNodes: Array<any>) {
		const storedNodes = this._entities.getValue();
		const updated: Entity[] = [...storedNodes];

		fetchedNodes.forEach((fetchedNode) => {
			const index = storedNodes.map((storedNode) => storedNode.id).indexOf(fetchedNode.id);

			if (index !== -1) {
				// If the node is already in the store, update it
				updated[index] = fetchedNode;
			} else {
				// If the node is not in the store, add it
				updated.push(fetchedNode);
			}
		});

		this._entities.next([...updated]);
	}
}
