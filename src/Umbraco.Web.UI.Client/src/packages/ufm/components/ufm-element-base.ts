import { UMB_UFM_CONTEXT } from '../contexts/ufm.context.js';
import { property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

export abstract class UmbUfmElementBase extends UmbLitElement {
	#filterFuncArgs?: Array<{ alias: string; args: Array<string> }>;

	#ufmContext?: typeof UMB_UFM_CONTEXT.TYPE;

	@property()
	public set filters(value: string | undefined) {
		this.#filters = value;

		this.#filterFuncArgs = value
			?.split('|')
			.filter((item) => item)
			.map((item) => {
				const [alias, ...args] = item.split(':').map((x) => x.trim());
				return { alias, args };
			});
	}
	public get filters(): string | undefined {
		return this.#filters;
	}
	#filters?: string;

	@state()
	value?: unknown;

	constructor() {
		super();

		this.consumeContext(UMB_UFM_CONTEXT, (ufmContext) => {
			this.#ufmContext = ufmContext;
		});
	}

	override render() {
		if (!this.isConnected) return;

		let values = Array.isArray(this.value) ? this.value : [this.value];

		if (this.#filterFuncArgs) {
			const missing = new Set<string>();

			for (const item of this.#filterFuncArgs) {
				const filter = this.#ufmContext?.getFilterByAlias(item.alias);
				if (filter) {
					values = values.map((value) => filter(value, ...item.args));
				} else {
					missing.add(item.alias);
				}
			}

			if (missing.size > 0) {
				console.warn(`UFM filters with aliases "${Array.from(missing).join('", "')}" were not found.`);
			}
		}

		return values.join(', ');
	}
}
