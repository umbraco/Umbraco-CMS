import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';

export class UmbRemoveDashboardApp extends UmbEntityActionBase<never> {
	override async execute() {
		alert('Delete Dashboard App');
	}
}

export { UmbRemoveDashboardApp as api };
