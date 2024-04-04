/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { DocumentTypeReferenceResponseModel } from './DocumentTypeReferenceResponseModel';
import type { ReferenceByIdModel } from './ReferenceByIdModel';

export type DocumentBlueprintTreeItemResponseModel = {
    hasChildren: boolean;
    id: string;
    parent?: ReferenceByIdModel | null;
    name: string;
    documentType: DocumentTypeReferenceResponseModel;
};

