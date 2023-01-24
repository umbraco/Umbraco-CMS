/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ConsentLevel } from './ConsentLevel';

export type UserSettings = {
    minCharLength?: number;
    minNonAlphaNumericLength?: number;
    consentLevels?: Array<ConsentLevel>;
};

