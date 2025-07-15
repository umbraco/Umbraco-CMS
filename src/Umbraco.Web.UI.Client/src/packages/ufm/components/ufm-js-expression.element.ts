import { UMB_UFM_CONTEXT } from '../contexts/ufm.context.js';
import { UMB_UFM_RENDER_CONTEXT } from './ufm-render/ufm-render.context.js';
import { customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { EvalAstFactory, Parser } from '@umbraco-cms/backoffice/external/heximal-expressions';
import { UmbLruCache } from '@umbraco-cms/backoffice/cache';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { Expression, Scope } from '@umbraco-cms/backoffice/external/heximal-expressions';

const astFactory = new EvalAstFactory();
const expressionCache = new UmbLruCache<string, Expression | undefined>(1000);

@customElement('umb-ufm-js-expression')
export class UmbUfmJsExpressionElement extends UmbLitElement {
	#ufmContext?: typeof UMB_UFM_CONTEXT.TYPE;

	@state()
	value?: unknown;

	constructor() {
		super();

		this.consumeContext(UMB_UFM_CONTEXT, (ufmContext) => {
			this.#ufmContext = ufmContext;
		});

		this.consumeContext(UMB_UFM_RENDER_CONTEXT, (context) => {
			this.observe(
				context?.value,
				(value) => {
					this.value = this.#labelTemplate(this.textContent ?? '', value);
				},
				'observeValue',
			);
		});
	}

	#labelTemplate(expression: string, model?: any): string {
		const filters = this.#ufmContext?.getFilters() ?? [];
		const functions = Object.fromEntries(filters.map((x) => [x.alias, x.filter]));
		const scope: Scope = { ...model, ...functions };

		let ast = expressionCache.get(expression);

		if (ast === undefined && !expressionCache.has(expression)) {
			try {
				ast = new Parser(expression, astFactory).parse();
			} catch {
				console.error(`Error parsing expression: \`${expression}\``);
			}
			expressionCache.set(expression, ast);
		}

		return ast?.evaluate(scope) ?? '';
	}

	override render() {
		return (Array.isArray(this.value) ? this.value : [this.value]).join(', ');
	}
}

export default UmbUfmJsExpressionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-ufm-js-expression': UmbUfmJsExpressionElement;
	}
}
