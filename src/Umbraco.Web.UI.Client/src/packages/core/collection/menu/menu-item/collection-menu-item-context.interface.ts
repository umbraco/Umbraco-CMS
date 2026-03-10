import type { UmbCollectionItemModel } from '../../item/types.js';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbContextMinimal } from '@umbraco-cms/backoffice/context-api';

export interface UmbCollectionMenuItemContext<
	CollectionMenuItemType extends UmbCollectionItemModel = UmbCollectionItemModel,
> extends UmbApi,
		UmbContextMinimal {
	item: Observable<CollectionMenuItemType | undefined>;
	isSelectable: Observable<boolean>;
	isSelected: Observable<boolean>;
	getItem(): CollectionMenuItemType | undefined;
	setItem(item: CollectionMenuItemType | undefined): void;
	select(): void;
	deselect(): void;
}
