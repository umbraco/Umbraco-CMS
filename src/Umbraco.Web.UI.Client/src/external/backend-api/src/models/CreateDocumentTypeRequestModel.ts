/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { CreateDocumentTypePropertyTypeContainerRequestModel } from './CreateDocumentTypePropertyTypeContainerRequestModel';
import type { CreateDocumentTypePropertyTypeRequestModel } from './CreateDocumentTypePropertyTypeRequestModel';
import type { DocumentTypeCleanupModel } from './DocumentTypeCleanupModel';
import type { DocumentTypeCompositionModel } from './DocumentTypeCompositionModel';
import type { DocumentTypeSortModel } from './DocumentTypeSortModel';
import type { ReferenceByIdModel } from './ReferenceByIdModel';

export type CreateDocumentTypeRequestModel = {
    alias: string;
    name: string;
    description?: string | null;
    icon: string;
    allowedAsRoot: boolean;
    variesByCulture: boolean;
    variesBySegment: boolean;
    collection?: ReferenceByIdModel | null;
    isElement: boolean;
    properties: Array<CreateDocumentTypePropertyTypeRequestModel>;
    containers: Array<CreateDocumentTypePropertyTypeContainerRequestModel>;
    id?: string | null;
    parent?: ReferenceByIdModel | null;
    allowedTemplates: Array<ReferenceByIdModel>;
    defaultTemplate?: ReferenceByIdModel | null;
    cleanup: DocumentTypeCleanupModel;
    allowedDocumentTypes: Array<DocumentTypeSortModel>;
    compositions: Array<DocumentTypeCompositionModel>;
};

