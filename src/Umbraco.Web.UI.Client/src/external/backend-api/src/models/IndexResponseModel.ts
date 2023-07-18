/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { HealthStatusModel } from './HealthStatusModel';

export type IndexResponseModel = {
    name: string;
    healthStatus?: HealthStatusModel;
    canRebuild: boolean;
    searcherName?: string;
    documentCount: number;
    fieldCount: number;
    providerProperties?: Record<string, any> | null;
};

