/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { PropertyTypeAppearanceModel } from './PropertyTypeAppearanceModel';
import type { PropertyTypeValidationModel } from './PropertyTypeValidationModel';

export type PropertyTypeModelBaseModel = {
    id?: string;
    containerId?: string | null;
    sortOrder?: number;
    alias?: string;
    name?: string;
    description?: string | null;
    dataTypeId?: string;
    variesByCulture?: boolean;
    variesBySegment?: boolean;
    validation?: PropertyTypeValidationModel;
    appearance?: PropertyTypeAppearanceModel;
};
