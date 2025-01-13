import type { UmbNamableWorkspaceContext } from '../types.js';
import { UmbEntityDetailWorkspaceContextBase } from './entity-detail-workspace-base.js';
import type { UmbEntityDetailWorkspaceContextCreateArgs } from './types.js';
import type { UmbNamedEntityModel } from '@umbraco-cms/backoffice/entity';
import type { UmbDetailRepository } from '@umbraco-cms/backoffice/repository';

export class UmbEntityNamedDetailWorkspaceContextBase
	extends UmbEntityDetailWorkspaceContextBase<
		UmbNamedEntityModel,
		UmbDetailRepository<UmbNamedEntityModel>,
		UmbEntityDetailWorkspaceContextCreateArgs<UmbNamedEntityModel>
	>
	implements UmbNamableWorkspaceContext
{
	// Just for context token safety:
	public readonly IS_ENTITY_NAMED_DETAIL_WORKSPACE_CONTEXT = true;
	readonly name = this._data.createObservablePartOfCurrent((data) => data?.name);

	getName() {
		return this._data.getCurrent()?.name;
	}

	setName(name: string | undefined) {
		this._data.updateCurrent({ name });
	}
}
