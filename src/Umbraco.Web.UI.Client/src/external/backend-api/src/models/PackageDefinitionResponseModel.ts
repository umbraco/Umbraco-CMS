/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { PackageModelBaseModel } from './PackageModelBaseModel';

export type PackageDefinitionResponseModel = (PackageModelBaseModel & {
id?: string;
packagePath?: string;
});
