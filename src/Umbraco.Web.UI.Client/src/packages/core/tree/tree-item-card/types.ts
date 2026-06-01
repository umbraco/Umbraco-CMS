import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import type { UmbTreeItemModel } from '../types.js';
import type { UmbTreeItemApi } from '../tree-item/tree-item-base/tree-item-api-base.js';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbTreeItemCardApi extends UmbTreeItemApi {}

export interface UmbTreeItemCardElement extends UmbControllerHostElement {
	item: UmbTreeItemModel | undefined;
	api: UmbTreeItemCardApi | undefined;
}

export type * from './tree-item-card.extension.js';
