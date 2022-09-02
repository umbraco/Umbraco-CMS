import { BehaviorSubject, Observable, map } from 'rxjs';
import { Entity } from '../../mocks/data/entity.data';
import { deepmerge } from 'deepmerge-ts';

export class UmbEntityStore {
	private _entities: BehaviorSubject<Array<Entity>> = new BehaviorSubject(<Array<Entity>>[]);
	public readonly entities: Observable<Array<Entity>> = this._entities.asObservable();

	getByKeys(keys: Array<string>): Observable<Array<Entity>> {
		return this.entities.pipe(
			map((entities: Array<Entity>) => entities.filter((entity: Entity) => keys.includes(entity.key)))
		);
	}

	update(entities: Array<any>) {
		this._updateStore(entities);
	}

	private _updateStore(updatedItems: Array<Entity>) {
		const storedItems = this._entities.getValue();
		const updated: Entity[] = [...storedItems];

		updatedItems.forEach((updatedItem) => {
			const index = storedItems.map((storedNode) => storedNode.key).indexOf(updatedItem.key);

			if (index !== -1) {
				const entityKeys = Object.keys(storedItems[index]);
				const mergedData = deepmerge(storedItems[index], updatedItem);

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
