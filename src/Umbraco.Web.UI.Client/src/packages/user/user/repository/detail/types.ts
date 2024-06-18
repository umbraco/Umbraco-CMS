import type { UmbUserDetailModel } from '../../types.js';
import type { UmbDataSourceResponse, UmbDetailDataSource } from '@umbraco-cms/backoffice/repository';

export interface UmbUserDetailDataSource extends UmbDetailDataSource<UmbUserDetailModel> {
	calculateStartNodes(unique: string): Promise<UmbDataSourceResponse<string[]>>;
}
