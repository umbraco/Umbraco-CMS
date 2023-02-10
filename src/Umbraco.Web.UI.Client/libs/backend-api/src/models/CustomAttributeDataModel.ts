/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { ConstructorInfoModel } from './ConstructorInfoModel';
import type { CustomAttributeNamedArgumentModel } from './CustomAttributeNamedArgumentModel';
import type { CustomAttributeTypedArgumentModel } from './CustomAttributeTypedArgumentModel';
import type { TypeModel } from './TypeModel';

export type CustomAttributeDataModel = {
    attributeType?: TypeModel;
    constructor?: ConstructorInfoModel;
    readonly constructorArguments?: Array<CustomAttributeTypedArgumentModel>;
    readonly namedArguments?: Array<CustomAttributeNamedArgumentModel>;
};

