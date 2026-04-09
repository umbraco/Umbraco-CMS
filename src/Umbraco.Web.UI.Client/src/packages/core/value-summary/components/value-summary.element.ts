import type { ManifestValueSummary } from '../extensions/value-summary.extension.js';
import { UMB_VALUE_SUMMARY_COORDINATOR_CONTEXT } from '../coordinator/value-summary-coordinator.context-token.js';
import { customElement, html, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-value-summary')
export class UmbValueSummaryElement extends UmbLitElement {
	@property({ attribute: false })
	set valueType(value: string | undefined) {
		this.#valueType = value;
		this.#register();
	}
	get valueType() {
		return this.#valueType;
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

	#valueType?: string;
	#rawValue?: unknown;

	#coordinator?: typeof UMB_VALUE_SUMMARY_COORDINATOR_CONTEXT.TYPE;

	constructor() {
		super();
		this.consumeContext(UMB_VALUE_SUMMARY_COORDINATOR_CONTEXT, (coordinator) => {
			this.#coordinator = coordinator;
			this.#register();
		});
	}

	#register() {
		if (!this.#valueType) return;
		if (this.#coordinator) {
			const valueType = this.#valueType;
			const rawValue = this.#rawValue;
			this.#coordinator.preRegister(valueType, [rawValue]);
			this.observe(
				this.#coordinator.observeResolvedValue(valueType, rawValue),
				(resolved) => (this._resolvedValue = resolved),
				'umbValueSummaryResolved',
			);
		} else {
			// No coordinator — pass raw value through to the extension slot
			this._resolvedValue = this.#rawValue;
		}
	}

	override render() {
		if (!this.#valueType) return nothing;
		return html`<umb-extension-slot
			type="valueSummary"
			single
			.filter=${(m: ManifestValueSummary) => m.forValueType === this.#valueType}
			.props=${{ value: this._resolvedValue }}>
			<span>${String(this._resolvedValue ?? '')}</span>
		</umb-extension-slot>`;
	}
}

export { UmbValueSummaryElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-value-summary': UmbValueSummaryElement;
	}
}
