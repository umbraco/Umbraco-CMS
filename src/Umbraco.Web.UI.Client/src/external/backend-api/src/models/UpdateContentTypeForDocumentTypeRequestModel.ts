/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { ReferenceByIdModel } from './ReferenceByIdModel';
import type { UpdateDocumentTypePropertyTypeContainerRequestModel } from './UpdateDocumentTypePropertyTypeContainerRequestModel';
import type { UpdateDocumentTypePropertyTypeRequestModel } from './UpdateDocumentTypePropertyTypeRequestModel';
export type UpdateContentTypeForDocumentTypeRequestModel = {
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
};

