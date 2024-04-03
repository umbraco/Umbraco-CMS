/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ReferenceByIdModel } from './ReferenceByIdModel';

export type DocumentVersionItemResponseModel = {
    id: string;
    document: ReferenceByIdModel;
    documentType: ReferenceByIdModel;
    user: ReferenceByIdModel;
    versionDate: string;
    isCurrentPublishedVersion: boolean;
    isCurrentDraftVersion: boolean;
    preventCleanup: boolean;
};

