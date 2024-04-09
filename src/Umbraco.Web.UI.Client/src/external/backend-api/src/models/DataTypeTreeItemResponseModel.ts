/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ReferenceByIdModel } from './ReferenceByIdModel';

export type DataTypeTreeItemResponseModel = {
    hasChildren: boolean;
    id: string;
    parent?: ReferenceByIdModel | null;
    name: string;
    isFolder: boolean;
    editorUiAlias?: string | null;
    isDeletable: boolean;
};

