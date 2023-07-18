/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ModelsModeModel } from './ModelsModeModel';

export type ModelsBuilderResponseModel = {
    mode?: ModelsModeModel;
    canGenerate?: boolean;
    outOfDateModels?: boolean;
    lastError?: string | null;
    version?: string | null;
    modelsNamespace?: string | null;
    trackingOutOfDateModels?: boolean;
};

