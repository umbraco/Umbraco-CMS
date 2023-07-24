/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ContentTypeCompositionModel } from './ContentTypeCompositionModel';
import type { ContentTypeSortModel } from './ContentTypeSortModel';
import type { DocumentTypePropertyTypeContainerResponseModel } from './DocumentTypePropertyTypeContainerResponseModel';
import type { DocumentTypePropertyTypeResponseModel } from './DocumentTypePropertyTypeResponseModel';

export type ContentTypeResponseModelBaseDocumentTypePropertyTypeResponseModelDocumentTypePropertyTypeContainerResponseModel = {
    alias?: string;
    name?: string;
    description?: string | null;
    icon?: string;
    allowedAsRoot?: boolean;
    variesByCulture?: boolean;
    variesBySegment?: boolean;
    isElement?: boolean;
    properties?: Array<DocumentTypePropertyTypeResponseModel>;
    containers?: Array<DocumentTypePropertyTypeContainerResponseModel>;
    allowedContentTypes?: Array<ContentTypeSortModel>;
    compositions?: Array<ContentTypeCompositionModel>;
    id?: string;
};

