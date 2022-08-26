import { BehaviorSubject, Observable } from 'rxjs';
import { Entity } from '../../mocks/data/entity.data';

export class UmbEntityStore {
	private _entities: BehaviorSubject<Array<Entity>> = new BehaviorSubject(<Array<Entity>>[]);
	public readonly entities: Observable<Array<Entity>> = this._entities.asObservable();

	update(entities: Array<Entity>) {
		this._updateStore(entities);
	}

	private _updateStore(fetchedNodes: Array<Entity>) {
		const storedNodes = this._entities.getValue();
		const updated: Entity[] = [...storedNodes];

		fetchedNodes.forEach((fetchedNode) => {
			const index = storedNodes.map((storedNode) => storedNode.key).indexOf(fetchedNode.key);

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
