/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { PropertyTypeAppearanceModel } from './PropertyTypeAppearanceModel';
import type { PropertyTypeValidationModel } from './PropertyTypeValidationModel';

export type PropertyTypeViewModelBaseModel = {
    key?: string;
    containerKey?: string | null;
    alias?: string;
    name?: string;
    description?: string | null;
    dataTypeKey?: string;
    variesByCulture?: boolean;
    variesBySegment?: boolean;
    validation?: PropertyTypeValidationModel;
    appearance?: PropertyTypeAppearanceModel;
};

