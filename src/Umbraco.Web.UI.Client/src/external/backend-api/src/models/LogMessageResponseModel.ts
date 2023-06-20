/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { LogLevelModel } from './LogLevelModel';
import type { LogMessagePropertyPresentationModel } from './LogMessagePropertyPresentationModel';

export type LogMessageResponseModel = {
    timestamp?: string;
    level?: LogLevelModel;
    messageTemplate?: string | null;
    renderedMessage?: string | null;
    properties?: Array<LogMessagePropertyPresentationModel>;
    exception?: string | null;
};
