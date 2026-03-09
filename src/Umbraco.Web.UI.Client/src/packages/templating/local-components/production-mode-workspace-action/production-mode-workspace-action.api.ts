import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import { umbInfoModal } from '@umbraco-cms/backoffice/modal';
import { UmbWorkspaceActionBase } from '@umbraco-cms/backoffice/workspace';

export class UmbTemplatingProductionModeWorkspaceActionApi extends UmbWorkspaceActionBase {
	#localize = new UmbLocalizationController(this);

	public override async execute(): Promise<void> {
		await umbInfoModal(this, {
			headline: this.#localize.term('template_productionMode'),
			content: this.#localize.term('template_runtimeModeProduction'),
		}).catch(() => undefined);
	}
}
