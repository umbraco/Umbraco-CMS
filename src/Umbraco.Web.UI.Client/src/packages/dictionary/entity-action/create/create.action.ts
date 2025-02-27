import { UMB_CREATE_DICTIONARY_WORKSPACE_PATH_PATTERN } from '../../workspace/index.js';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';

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
