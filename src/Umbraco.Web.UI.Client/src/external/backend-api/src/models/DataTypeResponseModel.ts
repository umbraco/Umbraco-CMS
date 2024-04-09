/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { DataTypePropertyPresentationModel } from './DataTypePropertyPresentationModel';

export type DataTypeResponseModel = {
    name: string;
    editorAlias: string;
    editorUiAlias?: string | null;
    values: Array<DataTypePropertyPresentationModel>;
    id: string;
    isDeletable: boolean;
    canIgnoreStartNodes: boolean;
};

