/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ContentTypeCompositionModel } from './ContentTypeCompositionModel';
import type { ContentTypeSortModel } from './ContentTypeSortModel';
import type { UpdateDocumentTypePropertyTypeContainerRequestModel } from './UpdateDocumentTypePropertyTypeContainerRequestModel';
import type { UpdateDocumentTypePropertyTypeRequestModel } from './UpdateDocumentTypePropertyTypeRequestModel';

export type UpdateContentTypeRequestModelBaseUpdateDocumentTypePropertyTypeRequestModelUpdateDocumentTypePropertyTypeContainerRequestModel = {
    alias?: string;
    name?: string;
    description?: string | null;
    icon?: string;
    allowedAsRoot?: boolean;
    variesByCulture?: boolean;
    variesBySegment?: boolean;
    isElement?: boolean;
    properties?: Array<UpdateDocumentTypePropertyTypeRequestModel>;
    containers?: Array<UpdateDocumentTypePropertyTypeContainerRequestModel>;
    allowedContentTypes?: Array<ContentTypeSortModel>;
    compositions?: Array<ContentTypeCompositionModel>;
};

