/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ContentTreeItemResponseModel } from './ContentTreeItemResponseModel';
import type { DataTypeTreeItemResponseModel } from './DataTypeTreeItemResponseModel';
import type { DocumentBlueprintTreeItemResponseModel } from './DocumentBlueprintTreeItemResponseModel';
import type { DocumentTreeItemResponseModel } from './DocumentTreeItemResponseModel';
import type { DocumentTypeTreeItemResponseModel } from './DocumentTypeTreeItemResponseModel';
import type { EntityTreeItemResponseModel } from './EntityTreeItemResponseModel';
import type { FolderTreeItemResponseModel } from './FolderTreeItemResponseModel';
import type { MediaTreeItemResponseModel } from './MediaTreeItemResponseModel';
import type { MediaTypeTreeItemResponseModel } from './MediaTypeTreeItemResponseModel';

export type PagedEntityTreeItemResponseModel = {
    total: number;
    items: Array<(EntityTreeItemResponseModel | ContentTreeItemResponseModel | DataTypeTreeItemResponseModel | DocumentBlueprintTreeItemResponseModel | DocumentTreeItemResponseModel | DocumentTypeTreeItemResponseModel | FolderTreeItemResponseModel | MediaTreeItemResponseModel | MediaTypeTreeItemResponseModel)>;
};

