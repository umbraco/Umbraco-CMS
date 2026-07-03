import { UmbWorkspaceActionBase } from '../workspace-action/workspace-action-base.controller.js';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import { umbInfoModal } from '@umbraco-cms/backoffice/modal';

/**
 * A workspace action that informs the user that editing is disabled because the server runs in
 * production runtime mode. Intended to replace the save action when the
 * `Umb.Condition.Server.IsProductionMode` condition matches.
 */
export class UmbProductionModeWorkspaceActionApi extends UmbWorkspaceActionBase {
	#localize = new UmbLocalizationController(this);

	public override async execute(): Promise<void> {
		await umbInfoModal(this, {
			headline: this.#localize.term('general_productionMode'),
			content: this.#localize.term('general_runtimeModeProduction'),
		}).catch(() => undefined);
	}
}

export { UmbProductionModeWorkspaceActionApi as api };
