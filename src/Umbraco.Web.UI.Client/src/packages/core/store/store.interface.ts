import { Observable } from '@umbraco-cms/backoffice/external/rxjs';

export interface UmbStore<T> extends EventTarget {
	append: (item: T) => void;
	appendItems: (items: Array<T>) => void;
	updateItem: (unique: string, item: Partial<T>) => void;
	removeItem: (unique: string) => void;
	removeItems: (uniques: Array<string>) => void;
	getItems: (uniques: Array<string>) => Array<T>;
	all: () => Observable<Array<T>>;
}
