/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { HealthStatus } from './HealthStatus';

export type Index = {
    name: string;
    healthStatus?: HealthStatus;
    canRebuild: boolean;
    searcherName?: string;
    documentCount: number;
    fieldCount: number;
    providerProperties?: Record<string, any> | null;
};

