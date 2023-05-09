/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { DatabaseSettingsPresentationModel } from './DatabaseSettingsPresentationModel';
import type { UserSettingsModel } from './UserSettingsModel';

export type InstallSettingsResponseModel = {
    user?: UserSettingsModel;
    databases?: Array<DatabaseSettingsPresentationModel>;
};

