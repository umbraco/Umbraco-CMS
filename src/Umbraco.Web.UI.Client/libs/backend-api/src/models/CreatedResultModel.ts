/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { AngularJsonMediaTypeFormatterModel } from './AngularJsonMediaTypeFormatterModel';
import type { NamedSystemTextJsonOutputFormatterModel } from './NamedSystemTextJsonOutputFormatterModel';
import type { TypeModel } from './TypeModel';

export type CreatedResultModel = {
    value?: any;
    formatters?: Array<(NamedSystemTextJsonOutputFormatterModel | AngularJsonMediaTypeFormatterModel)>;
    contentTypes?: Array<string>;
    declaredType?: TypeModel;
    statusCode?: number | null;
    location?: string;
};

