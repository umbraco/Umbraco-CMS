import { BehaviorSubject, Observable } from 'rxjs';
import { Entity } from '../../mocks/data/entity.data';

export class UmbEntityStore {
	private _entities: BehaviorSubject<Array<Entity>> = new BehaviorSubject(<Array<Entity>>[]);
	public readonly entities: Observable<Array<Entity>> = this._entities.asObservable();

	update(entities: Array<any>) {
		this._updateStore(entities);
	}

	private _updateStore(updatedItems: Array<Entity>) {
		const storedItems = this._entities.getValue();
		const updated: Entity[] = [...storedItems];

		updatedItems.forEach((updatedItem) => {
			const index = storedItems.map((storedNode) => storedNode.key).indexOf(updatedItem.key);

			if (index !== -1) {
				// TODO consider deep merge
				const entityKeys = Object.keys(storedItems[index]);
				const mergedData = Object.assign(storedItems[index], updatedItem);

				for (const [key] of Object.entries(mergedData)) {
					if (entityKeys.indexOf(key) === -1) {
						// eslint-disable-next-line @typescript-eslint/ban-ts-comment
						// @ts-ignore
						delete mergedData[key];
					}
				}

				updated[index] = mergedData;
			} else {
				// If the node is not in the store, add it
				updated.push(updatedItem);
			}
		});

		this._entities.next([...updated]);
	}
}
