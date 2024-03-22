/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ObjectTypeResponseModel } from './ObjectTypeResponseModel';
import type { RelationTypeBaseModel } from './RelationTypeBaseModel';

export type RelationTypeResponseModel = (RelationTypeBaseModel & {
    id: string;
    alias?: string | null;
    parentObject?: ObjectTypeResponseModel | null;
    childObject?: ObjectTypeResponseModel | null;
});

