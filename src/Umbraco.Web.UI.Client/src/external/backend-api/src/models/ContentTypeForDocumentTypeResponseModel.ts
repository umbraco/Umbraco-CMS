/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { DocumentTypePropertyTypeContainerResponseModel } from './DocumentTypePropertyTypeContainerResponseModel';
import type { DocumentTypePropertyTypeResponseModel } from './DocumentTypePropertyTypeResponseModel';
import type { ReferenceByIdModel } from './ReferenceByIdModel';

export type ContentTypeForDocumentTypeResponseModel = {
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
};

