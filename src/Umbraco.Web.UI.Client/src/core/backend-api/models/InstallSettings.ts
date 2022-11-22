/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { DatabaseSettings } from './DatabaseSettings';
import type { UserSettings } from './UserSettings';

export type InstallSettings = {
    user?: UserSettings;
    databases?: Array<DatabaseSettings> | null;
};

