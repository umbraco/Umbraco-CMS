import { Observable } from 'rxjs';
import type { UmbEntityStore } from '../../core/stores/entity.store';
import { Entity } from '../../mocks/data/entity.data';

export interface UmbTreeContext {
	entityStore: UmbEntityStore;
	selectable: Observable<boolean>;
	selection: Observable<Array<string>>;
	fetchRoot?(): Observable<Entity[]>;
	fetchChildren?(key: string): Observable<Entity[]>;
	setSelectable(value: boolean): void;
	select(key: string): void;
}
