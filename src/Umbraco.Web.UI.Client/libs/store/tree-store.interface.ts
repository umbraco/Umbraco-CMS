import type { Observable } from 'rxjs';
import { TreeItemPresentationModel } from '../backend-api';

export interface UmbTreeStore<T extends TreeItemPresentationModel = any> {
	appendItems: (items: Array<T>) => void;
	updateItem: (unique: string, item: Partial<T>) => void;
	removeItem: (unique: string) => void;

	rootItems: Observable<Array<T>>;
	childrenOf: (parentUnique: string | null) => Observable<Array<T>>;
	items: (uniques: Array<string>) => Observable<Array<T>>;
}
