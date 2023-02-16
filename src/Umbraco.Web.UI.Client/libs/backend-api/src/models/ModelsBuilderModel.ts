/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ModelsModeModel } from './ModelsModeModel';

export type ModelsBuilderModel = {
    mode?: ModelsModeModel;
    canGenerate?: boolean;
    outOfDateModels?: boolean;
    lastError?: string | null;
    version?: string | null;
    modelsNamespace?: string | null;
    trackingOutOfDateModels?: boolean;
};
