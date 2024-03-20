/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { RelationTypeBaseModel } from './RelationTypeBaseModel';

export type RelationTypeResponseModel = (RelationTypeBaseModel & {
    id: string;
    alias?: string | null;
    isDeletable: boolean;
    parentObjectTypeName?: string | null;
    childObjectTypeName?: string | null;
});

