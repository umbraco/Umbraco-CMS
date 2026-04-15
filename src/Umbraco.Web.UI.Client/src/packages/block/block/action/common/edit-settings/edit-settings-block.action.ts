import type { MetaBlockActionDefaultKind } from '../../default/types.js';
import type { UmbBlockActionArgs } from '../../types.js';
import { UmbBlockActionBase } from '../../block-action-base.js';
import { UMB_BLOCK_ENTRY_CONTEXT } from '../../../context/block-entry.context-token.js';
import { UmbDataPathBlockElementDataQuery } from '@umbraco-cms/backoffice/block';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbEditSettingsBlockAction extends UmbBlockActionBase<MetaBlockActionDefaultKind> {
	#context?: typeof UMB_BLOCK_ENTRY_CONTEXT.TYPE;
	#contextReady: Promise<void>;
	#resolveContext!: () => void;

	constructor(host: UmbControllerHost, args: UmbBlockActionArgs<MetaBlockActionDefaultKind>) {
		super(host, args);

		this.#contextReady = new Promise<void>((resolve) => {
			this.#resolveContext = resolve;
		});

		this.consumeContext(UMB_BLOCK_ENTRY_CONTEXT, (context) => {
			this.#context = context;
			this.#resolveContext();
		});
	}

	override async getHref() {
		await this.#contextReady;
		return this.observe(this.#context?.workspaceEditSettingsPath)?.asPromise();
	}

	override async getValidationDataPath() {
		await this.#contextReady;
		const settingsKey = await this.observe(this.#context?.settingsKey)?.asPromise();
		if (!settingsKey) return undefined;
		return `$.settingsData[${UmbDataPathBlockElementDataQuery({ key: settingsKey })}]`;
	}
}

export { UmbEditSettingsBlockAction as api };
