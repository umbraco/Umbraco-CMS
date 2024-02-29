/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { DataTypeTreeItemResponseModel } from './DataTypeTreeItemResponseModel';
import type { DocumentBlueprintTreeItemResponseModel } from './DocumentBlueprintTreeItemResponseModel';
import type { DocumentTypeTreeItemResponseModel } from './DocumentTypeTreeItemResponseModel';
import type { FolderTreeItemResponseModel } from './FolderTreeItemResponseModel';
import type { MediaTypeTreeItemResponseModel } from './MediaTypeTreeItemResponseModel';
import type { NamedEntityTreeItemResponseModel } from './NamedEntityTreeItemResponseModel';
export type PagedNamedEntityTreeItemResponseModel = {
    total: number;
    items: Array<(NamedEntityTreeItemResponseModel | DataTypeTreeItemResponseModel | DocumentBlueprintTreeItemResponseModel | DocumentTypeTreeItemResponseModel | FolderTreeItemResponseModel | MediaTypeTreeItemResponseModel)>;
};

