/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { ReferenceByIdModel } from './ReferenceByIdModel';
export type ContentTreeItemResponseModel = {
    type: string;
    hasChildren: boolean;
    parent?: ReferenceByIdModel | null;
    noAccess: boolean;
    isTrashed: boolean;
    id: string;
};

