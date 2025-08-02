import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

export interface UmbDashboardAppDetailModel extends UmbEntityModel {
	name: string;
}

export type * from './dashboard-app.extension.js';
