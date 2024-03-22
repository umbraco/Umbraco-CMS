/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ReferenceByIdModel } from './ReferenceByIdModel';
import type { RelationReferenceModel } from './RelationReferenceModel';

export type RelationResponseModel = {
    id: string;
    relationType: ReferenceByIdModel;
    parent: RelationReferenceModel;
    child: RelationReferenceModel;
    createDate: string;
    comment?: string | null;
};

