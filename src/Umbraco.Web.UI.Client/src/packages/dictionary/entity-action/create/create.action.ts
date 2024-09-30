import type { UmbEntityActionArgs } from '@umbraco-cms/backoffice/entity-action';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_CREATE_DICTIONARY_WORKSPACE_PATH_PATTERN } from '../../workspace/paths';

export class UmbCreateDictionaryEntityAction extends UmbEntityActionBase<never> {
	override async execute() {
		const createPath = UMB_CREATE_DICTIONARY_WORKSPACE_PATH_PATTERN.generateAbsolute({
			parentEntityType: this.args.entityType,
			parentUnique: this.args.unique ?? 'null',
		});

		history.pushState({}, '', createPath);
	}
}

export default UmbCreateDictionaryEntityAction;
