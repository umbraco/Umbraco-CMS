/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { Assembly } from './Assembly';
import type { CustomAttributeData } from './CustomAttributeData';
import type { ModuleHandle } from './ModuleHandle';

export type Module = {
    assembly?: Assembly;
    readonly fullyQualifiedName?: string | null;
    readonly name?: string | null;
    readonly mdStreamVersion?: number;
    readonly moduleVersionId?: string;
    readonly scopeName?: string | null;
    moduleHandle?: ModuleHandle;
    readonly customAttributes?: Array<CustomAttributeData> | null;
    readonly metadataToken?: number;
};

