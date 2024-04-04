/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { DocumentTypeCleanupModel } from './DocumentTypeCleanupModel';
import type { DocumentTypeCompositionModel } from './DocumentTypeCompositionModel';
import type { DocumentTypePropertyTypeContainerResponseModel } from './DocumentTypePropertyTypeContainerResponseModel';
import type { DocumentTypePropertyTypeResponseModel } from './DocumentTypePropertyTypeResponseModel';
import type { DocumentTypeSortModel } from './DocumentTypeSortModel';
import type { ReferenceByIdModel } from './ReferenceByIdModel';

export type DocumentTypeResponseModel = {
    alias: string;
    name: string;
    description?: string | null;
    icon: string;
    allowedAsRoot: boolean;
    variesByCulture: boolean;
    variesBySegment: boolean;
    collection?: ReferenceByIdModel | null;
    isElement: boolean;
    properties: Array<DocumentTypePropertyTypeResponseModel>;
    containers: Array<DocumentTypePropertyTypeContainerResponseModel>;
    id: string;
    allowedTemplates: Array<ReferenceByIdModel>;
    defaultTemplate?: ReferenceByIdModel | null;
    cleanup: DocumentTypeCleanupModel;
    allowedDocumentTypes: Array<DocumentTypeSortModel>;
    compositions: Array<DocumentTypeCompositionModel>;
};

