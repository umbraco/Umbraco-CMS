import type { UmbNamableWorkspaceContext } from '../types.js';
import { UmbNameWriteGuardManager } from '../namable/index.js';
import { UmbEntityDetailWorkspaceContextBase } from './entity-detail-workspace-base.js';
import type { UmbEntityDetailWorkspaceContextArgs, UmbEntityDetailWorkspaceContextCreateArgs } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbDetailRepository } from '@umbraco-cms/backoffice/repository';
import type { UmbNamedEntityModel } from '@umbraco-cms/backoffice/entity';

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

	public readonly name = this._data.createObservablePartOfCurrent((data) => data?.name);

	public readonly nameWriteGuard = new UmbNameWriteGuardManager(this);

	constructor(host: UmbControllerHost, args: UmbEntityDetailWorkspaceContextArgs) {
		super(host, args);
		this.nameWriteGuard.fallbackToPermitted();
		this.observe(
			this.name,
			(name) => {
				this.view.setTitle(name);
			},
			null,
		);
	}

	getName() {
		return this._data.getCurrent()?.name;
	}

	setName(name: string | undefined) {
		// We have to cast to Partial because TypeScript doesn't understand that the model has a name property due to generic sub-types
		this._data.updateCurrent({ name } as Partial<NamedDetailModelType>);
	}
}
