/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { RelationTypeBaseModel } from './RelationTypeBaseModel';

export type RelationTypeResponseModel = (RelationTypeBaseModel & {
key?: string;
alias?: string | null;
path?: string;
isSystemRelationType?: boolean;
parentObjectTypeName?: string | null;
childObjectTypeName?: string | null;
});
