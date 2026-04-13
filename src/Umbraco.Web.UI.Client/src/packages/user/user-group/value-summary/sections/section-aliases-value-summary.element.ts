import type { UmbValueSummaryApi, UmbValueSummaryElementInterface } from '@umbraco-cms/backoffice/value-summary';
import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-section-aliases-value-summary')
export class UmbSectionAliasesValueSummaryElement extends UmbLitElement implements UmbValueSummaryElementInterface {
	@property({ attribute: false })
	set api(api: UmbValueSummaryApi | undefined) {
		this.#api = api;
		if (api) {
			this.observe(
				api.value,
				(v) => {
					this.#value = v as string[] | undefined;
					this.#observeSectionNames();
				},
				'value',
			);
		}
	}
	get api() {
		return this.#api;
	}

	@state()
	private _sectionNames: Array<string> = [];

	#api?: UmbValueSummaryApi;
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
