/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { Assembly } from './Assembly';
import type { CustomAttributeData } from './CustomAttributeData';
import type { ModuleHandle } from './ModuleHandle';

export type Module = {
    assembly?: Assembly;
    readonly fullyQualifiedName?: string;
    readonly name?: string;
    readonly mdStreamVersion?: number;
    readonly moduleVersionId?: string;
    readonly scopeName?: string;
    moduleHandle?: ModuleHandle;
    readonly customAttributes?: Array<CustomAttributeData>;
    readonly metadataToken?: number;
};

