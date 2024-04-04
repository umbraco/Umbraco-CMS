/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { DocumentTypeCleanupModel } from './DocumentTypeCleanupModel';
import type { DocumentTypeCompositionModel } from './DocumentTypeCompositionModel';
import type { DocumentTypeSortModel } from './DocumentTypeSortModel';
import type { ReferenceByIdModel } from './ReferenceByIdModel';
import type { UpdateDocumentTypePropertyTypeContainerRequestModel } from './UpdateDocumentTypePropertyTypeContainerRequestModel';
import type { UpdateDocumentTypePropertyTypeRequestModel } from './UpdateDocumentTypePropertyTypeRequestModel';

export type UpdateDocumentTypeRequestModel = {
    alias: string;
    name: string;
    description?: string | null;
    icon: string;
    allowedAsRoot: boolean;
    variesByCulture: boolean;
    variesBySegment: boolean;
    collection?: ReferenceByIdModel | null;
    isElement: boolean;
    properties: Array<UpdateDocumentTypePropertyTypeRequestModel>;
    containers: Array<UpdateDocumentTypePropertyTypeContainerRequestModel>;
    allowedTemplates: Array<ReferenceByIdModel>;
    defaultTemplate?: ReferenceByIdModel | null;
    cleanup: DocumentTypeCleanupModel;
    allowedDocumentTypes: Array<DocumentTypeSortModel>;
    compositions: Array<DocumentTypeCompositionModel>;
};

