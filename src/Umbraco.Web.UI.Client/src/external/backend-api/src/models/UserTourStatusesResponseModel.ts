/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */

import type { SetTourStatusRequestModel } from './SetTourStatusRequestModel';
import type { TourStatusModel } from './TourStatusModel';

export type UserTourStatusesResponseModel = {
    tourStatuses?: Array<(TourStatusModel | SetTourStatusRequestModel)>;
};

