import type { Observable } from 'rxjs';
import { TreeItemPresentationModel } from '../backend-api';

export interface UmbTreeStore<T extends TreeItemPresentationModel = TreeItemPresentationModel> {
	appendItems: (items: Array<T>) => void;
	updateItem: (unique: string, item: Partial<T>) => void;
	removeItem: (unique: string) => void;

	rootItems: () => Observable<Array<T>>;
	childrenOf: (parentUnique: string | null) => Observable<Array<T>>;
	treeItems: (uniques: Array<string>) => Observable<Array<T>>;
}
