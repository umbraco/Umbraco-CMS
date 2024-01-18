import { UmbBlockManagerContext } from '@umbraco-cms/backoffice/block';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbId } from '@umbraco-cms/backoffice/id';
import { buildUdi } from '@umbraco-cms/backoffice/utils';

export class UmbBlockListManagerContext extends UmbBlockManagerContext {
	createBlock(contentElementTypeKey: string) {
		return super._createBlockData({}, contentElementTypeKey);
	}
}

export const UMB_BLOCK_LIST_MANAGER_CONTEXT = new UmbContextToken<
	UmbBlockListManagerContext,
	UmbBlockListManagerContext
>('UmbBlockManagerContext');
