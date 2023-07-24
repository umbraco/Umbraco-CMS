/* generated using openapi-typescript-codegen -- do no edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { AuditTypeModel } from './AuditTypeModel';

export type AuditLogBaseModel = {
    userId?: string;
    entityId?: string | null;
    timestamp?: string;
    logType?: AuditTypeModel;
    entityType?: string | null;
    comment?: string | null;
    parameters?: string | null;
};

