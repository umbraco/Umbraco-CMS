import { Observable } from 'rxjs';
import type { UmbEntityStore } from '../../core/stores/entity.store';
import type { ManifestTree } from '../../core/models';
import { Entity } from '../../mocks/data/entity.data';

export interface UmbTreeContext {
	tree: ManifestTree;
	entityStore: UmbEntityStore;
	fetchRoot(): Observable<Entity[]>;
	fetchChildren(key: string): Observable<Entity[]>;
}
