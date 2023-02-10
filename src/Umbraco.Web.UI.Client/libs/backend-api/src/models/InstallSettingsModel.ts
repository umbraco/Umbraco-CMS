/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { DatabaseSettingsModel } from './DatabaseSettingsModel';
import type { UserSettingsModel } from './UserSettingsModel';

export type InstallSettingsModel = {
    user?: UserSettingsModel;
    databases?: Array<DatabaseSettingsModel>;
};

