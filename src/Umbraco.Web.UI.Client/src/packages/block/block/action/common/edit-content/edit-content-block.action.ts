import type { MetaBlockActionDefaultKind } from '../../default/types.js';
import type { UmbBlockActionArgs } from '../../types.js';
import { UmbBlockActionBase } from '../../block-action-base.js';
import { UmbDataPathBlockElementDataQuery } from '../../../validation/data-path-element-data-query.function.js';
import { UMB_BLOCK_ENTRY_CONTEXT } from '../../../context/block-entry.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbStringState } from '@umbraco-cms/backoffice/observable-api';

/** Block action that navigates to the block's content editor workspace. */
export class UmbEditContentBlockAction extends UmbBlockActionBase<MetaBlockActionDefaultKind> {
	#contextReady: Promise<void>;
	#resolveContext!: () => void;

	readonly #href = new UmbStringState(undefined);
	readonly href = this.#href.asObservable();

	readonly #validationDataPath = new UmbStringState(undefined);
	readonly validationDataPath = this.#validationDataPath.asObservable();

	constructor(host: UmbControllerHost, args: UmbBlockActionArgs<MetaBlockActionDefaultKind>) {
		super(host, args);

		this.#contextReady = new Promise<void>((resolve) => {
			this.#resolveContext = resolve;
		});

		this.consumeContext(UMB_BLOCK_ENTRY_CONTEXT, (context) => {
			if (!context) return;
			this.#resolveContext();

			this.observe(context.workspaceEditContentPath, (path) => this.#href.setValue(path || undefined), 'observeHref');

			this.observe(
				context.contentKey,
				(contentKey) => {
					this.#validationDataPath.setValue(
						contentKey ? `$.contentData[${UmbDataPathBlockElementDataQuery({ key: contentKey })}]` : undefined,
					);
				},
				'observeValidationDataPath',
			);
		});
	}

	override async getHref() {
		await this.#contextReady;
		return (await this.observe(this.href)?.asPromise()) || undefined;
	}

	override async getValidationDataPath() {
		await this.#contextReady;
		return await this.observe(this.validationDataPath)?.asPromise();
	}
}

export { UmbEditContentBlockAction as api };
