import type { MetaBlockActionDefaultKind } from '../../default/types.js';
import type { UmbBlockActionArgs } from '../../types.js';
import { UmbBlockActionBase } from '../../block-action-base.js';
import { UMB_BLOCK_ENTRY_CONTEXT } from '../../../context/block-entry.context-token.js';
import { UmbDataPathBlockElementDataQuery } from '@umbraco-cms/backoffice/block';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbEditContentBlockAction extends UmbBlockActionBase<MetaBlockActionDefaultKind> {
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
		return this.observe(this.#context?.workspaceEditContentPath)?.asPromise();
	}

	override async getValidationDataPath() {
		await this.#contextReady;
		const contentKey = await this.observe(this.#context?.contentKey)?.asPromise();
		if (!contentKey) return undefined;
		return `$.contentData[${UmbDataPathBlockElementDataQuery({ key: contentKey })}]`;
	}
}

export { UmbEditContentBlockAction as api };
