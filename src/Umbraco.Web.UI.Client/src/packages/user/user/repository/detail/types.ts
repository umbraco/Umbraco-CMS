import type { UmbUserDetailModel, UmbUserStartNodesModel } from '../../types.js';
import type { UmbDataSourceResponse, UmbDetailDataSource } from '@umbraco-cms/backoffice/repository';

export interface UmbUserDetailDataSource extends UmbDetailDataSource<UmbUserDetailModel> {
	calculateStartNodes(unique: string): Promise<UmbDataSourceResponse<UmbUserStartNodesModel>>;
}
