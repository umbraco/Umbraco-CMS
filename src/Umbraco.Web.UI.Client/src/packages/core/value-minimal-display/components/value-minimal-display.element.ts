import type { ManifestValueMinimalDisplay } from '../extensions/value-minimal-display.extension.js';
import { UMB_VALUE_MINIMAL_DISPLAY_COORDINATOR_CONTEXT } from '../coordinator/value-minimal-display-coordinator.context-token.js';
import { customElement, html, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-value-minimal-display')
export class UmbValueMinimalDisplayElement extends UmbLitElement {
	@property({ attribute: false })
	set alias(value: string | undefined) {
		this.#alias = value;
		this.#register();
	}
	get alias() {
		return this.#alias;
	}

	@property({ attribute: false })
	set value(value: unknown) {
		this.#rawValue = value;
		this.#register();
	}
	get value() {
		return this.#rawValue;
	}

	@state()
	private _resolvedValue?: unknown;

	#alias?: string;
	#rawValue?: unknown;

	#coordinator?: typeof UMB_VALUE_MINIMAL_DISPLAY_COORDINATOR_CONTEXT.TYPE;

	constructor() {
		super();
		this.consumeContext(UMB_VALUE_MINIMAL_DISPLAY_COORDINATOR_CONTEXT, (coordinator) => {
			this.#coordinator = coordinator;
			this.#register();
		});
	}

	#register() {
		if (!this.#alias) return;
		if (!this.#coordinator) {
			this._resolvedValue = this.#rawValue;
			return;
		}
		const alias = this.#alias;
		const rawValue = this.#rawValue;
		this.#coordinator.preRegister(alias, [rawValue]);
		this.observe(
			this.#coordinator.observeResolvedValue(alias, rawValue),
			(resolved) => (this._resolvedValue = resolved),
			'umbValueMinimalDisplayResolved',
		);
	}

	override render() {
		if (!this.#alias) return nothing;
		return html`<umb-extension-slot
			type="valueMinimalDisplay"
			single
			.filter=${(m: ManifestValueMinimalDisplay) => m.alias === this.#alias}
			.props=${{ value: this._resolvedValue }}>
			<span>${String(this._resolvedValue ?? '')}</span>
		</umb-extension-slot>`;
	}
}

export { UmbValueMinimalDisplayElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-value-minimal-display': UmbValueMinimalDisplayElement;
	}
}
