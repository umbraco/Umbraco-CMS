/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { EncodingModel } from './EncodingModel';

export type NewtonsoftJsonOutputFormatterModel = {
    $type: string;
    readonly supportedMediaTypes?: Array<string>;
    readonly supportedEncodings?: Array<EncodingModel>;
};

