import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-section-aliases-value-summary')
export class UmbSectionAliasesValueSummaryElement extends UmbLitElement {
	@property({ attribute: false })
	set value(val: string[] | undefined) {
		this.#value = val;
		this.#observeSectionNames();
	}
	get value() {
		return this.#value;
	}

	@state()
	private _sectionNames: Array<string> = [];

	#value?: string[];

	#observeSectionNames() {
		if (!this.#value?.length) {
			this._sectionNames = [];
			return;
		}
		this.observe(
			umbExtensionsRegistry.byType('section'),
			(sections) => {
				this._sectionNames = sections
					.filter((x) => this.#value!.includes(x.alias))
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
