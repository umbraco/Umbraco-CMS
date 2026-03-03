import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import { umbConfirmModal } from '@umbraco-cms/backoffice/modal';
import { UmbWorkspaceActionBase } from '@umbraco-cms/backoffice/workspace';

export class UmbTemplatingProductionModeWorkspaceActionApi extends UmbWorkspaceActionBase {
	#localize = new UmbLocalizationController(this);

	public override async execute(): Promise<void> {
		await umbConfirmModal(this, {
			headline: this.#localize.term('template_productionMode'),
			content: this.#localize.term('template_runtimeModeProduction'),
			confirmLabel: this.#localize.term('general_close'),
		}).catch(() => undefined);
	}
}
