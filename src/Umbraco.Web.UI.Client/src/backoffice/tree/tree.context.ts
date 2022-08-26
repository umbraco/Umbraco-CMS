import { Observable } from 'rxjs';
import type { UmbExtensionManifestTree } from '../../core/extension';
import type { UmbEntityStore } from '../../core/stores/entity.store';
import { Entity } from '../../mocks/data/entity.data';

export interface UmbTreeContext {
	tree: UmbExtensionManifestTree;
	entityStore: UmbEntityStore;
	fetchRoot(): Observable<Entity[]>;
	fetchChildren(key: string): Observable<Entity[]>;
}
