import { html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbValueSummaryElementBase } from '@umbraco-cms/backoffice/value-summary';
import type { PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-section-aliases-value-summary')
export class UmbSectionAliasesValueSummaryElement extends UmbValueSummaryElementBase<string[]> {
	@state()
	private _sectionNames: Array<string> = [];

	protected override willUpdate(changedProperties: PropertyValueMap<this>): void {
		super.willUpdate(changedProperties);
		if (changedProperties.has('_value' as keyof this)) {
			this.#observeSectionNames();
		}
	}

	#observeSectionNames() {
		if (!this._value?.length) {
			this._sectionNames = [];
			return;
		}
		this.observe(
			umbExtensionsRegistry.byType('section'),
			(sections) => {
				this._sectionNames = sections
					.filter((x) => this._value!.includes(x.alias))
					.map((x) => (x.meta.label ? this.localize.string(x.meta.label) : x.name));
			},
			'umbSectionAliasesObserver',
		);
	}

	override render() {
		return html`${this._sectionNames.join(', ')}`;
	}
}

export { UmbSectionAliasesValueSummaryElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-section-aliases-value-summary': UmbSectionAliasesValueSummaryElement;
	}
}
