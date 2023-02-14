/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ContentTreeItemModel } from './ContentTreeItemModel';
import type { DocumentBlueprintTreeItemModel } from './DocumentBlueprintTreeItemModel';
import type { DocumentTreeItemModel } from './DocumentTreeItemModel';
import type { DocumentTypeTreeItemModel } from './DocumentTypeTreeItemModel';
import type { EntityTreeItemModel } from './EntityTreeItemModel';
import type { FolderTreeItemModel } from './FolderTreeItemModel';

export type PagedEntityTreeItemModel = {
    total: number;
    items: Array<(EntityTreeItemModel | ContentTreeItemModel | DocumentBlueprintTreeItemModel | DocumentTreeItemModel | DocumentTypeTreeItemModel | FolderTreeItemModel)>;
};

