import { dataSet } from './sets/index.js';
import type {
	HealthCheckGroupPresentationModel,
	HealthCheckGroupWithResultResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';

export const healthGroups: Array<HealthCheckGroupWithResultResponseModel & { name: string }> = dataSet.healthGroups;

export const healthGroupsWithoutResult: HealthCheckGroupPresentationModel[] = dataSet.healthGroupsWithoutResult;

export const getGroupByName = dataSet.getGroupByName;

export const getGroupWithResultsByName = dataSet.getGroupWithResultsByName;
