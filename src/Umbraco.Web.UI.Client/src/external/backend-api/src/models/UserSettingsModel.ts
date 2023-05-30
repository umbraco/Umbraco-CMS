/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ConsentLevelPresentationModel } from './ConsentLevelPresentationModel';

export type UserSettingsModel = {
    minCharLength?: number;
    minNonAlphaNumericLength?: number;
    consentLevels?: Array<ConsentLevelPresentationModel>;
};

