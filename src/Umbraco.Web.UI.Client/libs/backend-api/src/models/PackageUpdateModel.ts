/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { PackageModelBaseModel } from './PackageModelBaseModel';

export type PackageUpdateModel = (PackageModelBaseModel & {
    packagePath?: string;
});

