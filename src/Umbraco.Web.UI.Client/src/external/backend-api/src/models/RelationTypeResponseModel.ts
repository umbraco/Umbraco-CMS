/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ObjectTypeResponseModel } from './ObjectTypeResponseModel';

export type RelationTypeResponseModel = {
    name: string;
    isBidirectional: boolean;
    isDependency: boolean;
    id: string;
    alias?: string | null;
    parentObject?: ObjectTypeResponseModel | null;
    childObject?: ObjectTypeResponseModel | null;
};

