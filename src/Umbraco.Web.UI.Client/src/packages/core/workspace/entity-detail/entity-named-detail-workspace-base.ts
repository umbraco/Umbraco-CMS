import type { UmbNamableWorkspaceContext } from '../types.js';
import { UmbEntityDetailWorkspaceContextBase } from './entity-detail-workspace-base.js';
import type { UmbEntityDetailWorkspaceContextCreateArgs } from './types.js';
import type { UmbNamedEntityModel } from '@umbraco-cms/backoffice/entity';
import type { UmbDetailRepository } from '@umbraco-cms/backoffice/repository';

export abstract class UmbEntityNamedDetailWorkspaceContextBase<
		NamedDetailModelType extends UmbNamedEntityModel = UmbNamedEntityModel,
		NamedDetailRepositoryType extends
			UmbDetailRepository<NamedDetailModelType> = UmbDetailRepository<NamedDetailModelType>,
		CreateArgsType extends
			UmbEntityDetailWorkspaceContextCreateArgs<NamedDetailModelType> = UmbEntityDetailWorkspaceContextCreateArgs<NamedDetailModelType>,
	>
	extends UmbEntityDetailWorkspaceContextBase<NamedDetailModelType, NamedDetailRepositoryType, CreateArgsType>
	implements UmbNamableWorkspaceContext
{
	// Just for context token safety:
	public readonly IS_ENTITY_NAMED_DETAIL_WORKSPACE_CONTEXT = true;

	readonly name = this._data.createObservablePartOfCurrent((data) => data?.name);

	getName() {
		return this._data.getCurrent()?.name;
	}

	setName(name: string | undefined) {
		// We have to cast to Partial because TypeScript doesn't understand that the model has a name property due to generic sub-types
		this._data.updateCurrent({ name } as Partial<NamedDetailModelType>);
	}
}
