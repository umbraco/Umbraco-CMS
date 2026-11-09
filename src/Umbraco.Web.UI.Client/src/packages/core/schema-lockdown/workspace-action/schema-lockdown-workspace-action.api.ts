import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import { umbInfoModal } from '@umbraco-cms/backoffice/modal';
import { UmbWorkspaceActionBase } from '@umbraco-cms/backoffice/workspace';

export class UmbSchemaLockdownWorkspaceActionApi extends UmbWorkspaceActionBase {
	readonly #localize = new UmbLocalizationController(this);

	public override async execute(): Promise<void> {
		await umbInfoModal(this, {
			headline: this.#localize.term('schemaLockdown_locked'),
			content: this.#localize.term('schemaLockdown_updateLockedDescription'),
		}).catch(() => undefined);
	}
}

export { UmbSchemaLockdownWorkspaceActionApi as api };
