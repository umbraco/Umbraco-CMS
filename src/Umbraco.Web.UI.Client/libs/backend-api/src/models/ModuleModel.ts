/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { AssemblyModel } from './AssemblyModel';
import type { CustomAttributeDataModel } from './CustomAttributeDataModel';
import type { ModuleHandleModel } from './ModuleHandleModel';

export type ModuleModel = {
    assembly?: AssemblyModel;
    readonly fullyQualifiedName?: string;
    readonly name?: string;
    readonly mdStreamVersion?: number;
    readonly moduleVersionId?: string;
    readonly scopeName?: string;
    moduleHandle?: ModuleHandleModel;
    readonly customAttributes?: Array<CustomAttributeDataModel>;
    readonly metadataToken?: number;
};

