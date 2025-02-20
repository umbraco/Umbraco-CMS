import { UMB_UFM_CONTEXT } from '../contexts/ufm.context.js';
import { nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

export abstract class UmbUfmElementBase extends UmbLitElement {
	#filterFuncArgs?: Array<{ alias: string; args: Array<string> }>;

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

	#ufmContext?: typeof UMB_UFM_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_UFM_CONTEXT, (ufmContext) => {
			this.#ufmContext = ufmContext;
		});
	}

	override render() {
		if (!this.#ufmContext) return nothing;

		let values = Array.isArray(this.value) ? this.value : [this.value];
		if (this.#filterFuncArgs) {
			for (const item of this.#filterFuncArgs) {
				const filter = this.#ufmContext.getFilterByAlias(item.alias);
				if (filter) {
					values = values.map((value) => filter(value, ...item.args));
				}
			}
		}

		return values.join(', ');
	}
}
