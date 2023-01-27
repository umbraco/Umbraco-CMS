/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { LogLevel } from './LogLevel';
import type { LogMessageProperty } from './LogMessageProperty';

export type LogMessage = {
    timestamp?: string;
    level?: LogLevel;
    messageTemplate?: string | null;
    renderedMessage?: string | null;
    properties?: Array<LogMessageProperty>;
    exception?: string | null;
};

