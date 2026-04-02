import { EXAMPLE_COUNTER_CONTEXT } from './counter-workspace-context.js';
import { customElement, html, state, LitElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';

@customElement('example-counter-status-footer-app')
export class ExampleCounterStatusFooterAppElement extends UmbElementMixin(LitElement) {
	@state()
	private _counter = 0;

	constructor() {
		super();
		this.#observeCounter();
	}

	async #observeCounter() {
		const context = await this.getContext(EXAMPLE_COUNTER_CONTEXT);
		if (!context) return;

		this.observe(context.counter, (counter: number) => {
			this._counter = counter;
		});
	}

	override render() {
		return html`<span>Counter: ${this._counter}</span>`;
	}
}

export default ExampleCounterStatusFooterAppElement;

declare global {
	interface HTMLElementTagNameMap {
		'example-counter-status-footer-app': ExampleCounterStatusFooterAppElement;
	}
}
