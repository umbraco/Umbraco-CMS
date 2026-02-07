import type { UmbRepositoryErrorResponse } from '../../../repository/types.js';
import type { UmbTreeItemModelBase } from '../../types.js';
import type { UmbMoveToRequestArgs } from './types.js';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbMoveRepository extends UmbApi {
	requestMoveTo(args: UmbMoveToRequestArgs): Promise<UmbRepositoryErrorResponse>;
	getSelectableFilter?(unique: string): Promise<(item: UmbTreeItemModelBase) => boolean>;
}
