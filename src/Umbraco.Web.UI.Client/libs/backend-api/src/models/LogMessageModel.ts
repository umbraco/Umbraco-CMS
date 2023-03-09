/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { LogLevelModel } from './LogLevelModel';
import type { LogMessagePropertyModel } from './LogMessagePropertyModel';

export type LogMessageModel = {
    timestamp?: string;
    level?: LogLevelModel;
    messageTemplate?: string | null;
    renderedMessage?: string | null;
    properties?: Array<LogMessagePropertyModel>;
    exception?: string | null;
};

