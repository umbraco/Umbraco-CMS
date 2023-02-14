/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { DecoderFallbackModel } from './DecoderFallbackModel';
import type { EncoderFallbackModel } from './EncoderFallbackModel';
import type { ReadOnlySpanByteModel } from './ReadOnlySpanByteModel';

export type EncodingModel = {
    preamble?: ReadOnlySpanByteModel;
    readonly bodyName?: string;
    readonly encodingName?: string;
    readonly headerName?: string;
    readonly webName?: string;
    readonly windowsCodePage?: number;
    readonly isBrowserDisplay?: boolean;
    readonly isBrowserSave?: boolean;
    readonly isMailNewsDisplay?: boolean;
    readonly isMailNewsSave?: boolean;
    readonly isSingleByte?: boolean;
    encoderFallback?: EncoderFallbackModel;
    decoderFallback?: DecoderFallbackModel;
    readonly isReadOnly?: boolean;
    readonly codePage?: number;
};

