import { UmbWorkspaceActionBase } from '../workspace-action/workspace-action-base.controller.js';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import { umbInfoModal } from '@umbraco-cms/backoffice/modal';

/**
 * A workspace action that informs the user that editing is disabled because the schema is locked
 * down. Intended to replace the save action when the
 * `Umb.Condition.SchemaLockdown.OperationAllowed` condition does not match.
 */
export class UmbSchemaLockdownWorkspaceActionApi extends UmbWorkspaceActionBase {
	#localize = new UmbLocalizationController(this);

	public override async execute(): Promise<void> {
		await umbInfoModal(this, {
			headline: this.#localize.term('schemaLockdown_headline'),
			content: this.#localize.term('schemaLockdown_notice'),
		}).catch(() => undefined);
	}
}

export { UmbSchemaLockdownWorkspaceActionApi as api };
