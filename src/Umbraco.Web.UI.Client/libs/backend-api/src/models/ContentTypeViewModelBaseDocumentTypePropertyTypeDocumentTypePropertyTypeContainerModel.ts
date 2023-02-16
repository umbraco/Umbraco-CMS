/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ContentTypeCleanupModel } from './ContentTypeCleanupModel';
import type { ContentTypeCompositionModel } from './ContentTypeCompositionModel';
import type { ContentTypeSortModel } from './ContentTypeSortModel';
import type { DocumentTypePropertyTypeContainerModel } from './DocumentTypePropertyTypeContainerModel';
import type { DocumentTypePropertyTypeModel } from './DocumentTypePropertyTypeModel';

export type ContentTypeViewModelBaseDocumentTypePropertyTypeDocumentTypePropertyTypeContainerModel = {
    key?: string;
    alias?: string;
    name?: string;
    description?: string | null;
    icon?: string;
    allowedAsRoot?: boolean;
    variesByCulture?: boolean;
    variesBySegment?: boolean;
    isElement?: boolean;
    properties?: Array<DocumentTypePropertyTypeModel>;
    containers?: Array<DocumentTypePropertyTypeContainerModel>;
    allowedContentTypes?: Array<ContentTypeSortModel>;
    compositions?: Array<ContentTypeCompositionModel>;
    cleanup?: ContentTypeCleanupModel;
};

