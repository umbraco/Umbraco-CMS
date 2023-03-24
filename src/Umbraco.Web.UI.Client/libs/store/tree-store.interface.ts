import type { Observable } from 'rxjs';

export interface UmbTreeStore<T> {
	appendItems: (items: Array<T>) => void;
	updateItem: (unique: string, item: Partial<T>) => void;
	removeItem: (unique: string) => void;

	rootItems: () => Observable<Array<T>>;
	childrenOf: (parentUnique: string | null) => Observable<Array<T>>;
	treeItems: (uniques: Array<string>) => Observable<Array<T>>;
}
