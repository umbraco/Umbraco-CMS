/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { AuditTypeModel } from './AuditTypeModel';

export type AuditLogBaseModel = {
    userKey?: string;
    entityKey?: string | null;
    timestamp?: string;
    logType?: AuditTypeModel;
    entityType?: string | null;
    comment?: string | null;
    parameters?: string | null;
};

