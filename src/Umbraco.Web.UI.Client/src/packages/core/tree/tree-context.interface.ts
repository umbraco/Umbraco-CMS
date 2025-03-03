import type { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbSelectionManager } from '@umbraco-cms/backoffice/utils';

// TODO: update interface
export interface UmbTreeContext extends UmbContextBase<UmbTreeContext> {
	selection: UmbSelectionManager;
}
