/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ModelsMode } from './ModelsMode';

export type ModelsBuilder = {
    mode?: ModelsMode;
    canGenerate?: boolean;
    outOfDateModels?: boolean;
    lastError?: string | null;
    version?: string | null;
    modelsNamespace?: string | null;
    trackingOutOfDateModels?: boolean;
};

