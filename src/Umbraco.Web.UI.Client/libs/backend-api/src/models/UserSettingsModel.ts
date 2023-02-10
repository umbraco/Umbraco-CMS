/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ConsentLevelModel } from './ConsentLevelModel';

export type UserSettingsModel = {
    minCharLength?: number;
    minNonAlphaNumericLength?: number;
    consentLevels?: Array<ConsentLevelModel>;
};

