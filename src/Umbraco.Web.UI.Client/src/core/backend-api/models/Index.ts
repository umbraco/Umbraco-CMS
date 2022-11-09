/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

export type Index = {
    name?: string | null;
    healthStatus?: string | null;
    readonly isHealthy?: boolean;
    canRebuild?: boolean;
    searcherName?: string | null;
    documentCount?: number;
    fieldCount?: number;
};

