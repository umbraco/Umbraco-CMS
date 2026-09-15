import { UmbWorkspaceSplitViewVariantSelectorElement } from './workspace-split-view-variant-selector.element.js';
import { expect, fixture, html } from '@open-wc/testing';
import type { UmbEntityVariantModel, UmbEntityVariantOptionModel } from '@umbraco-cms/backoffice/variant';

type VariantOption = UmbEntityVariantOptionModel<UmbEntityVariantModel>;

const sorterA = (a: VariantOption, b: VariantOption) => (a.culture ?? '').localeCompare(b.culture ?? '');

// Exposes the protected, deprecated `_variantSorter` field for test assertions — `protected` (unlike `#private`)
// is reachable from a subclass.
class UmbTestVariantSelectorElement extends UmbWorkspaceSplitViewVariantSelectorElement {
	get deprecatedVariantSorter() {
		return this._variantSorter;
	}
	set deprecatedVariantSorter(value) {
		this._variantSorter = value;
	}
}
customElements.define('umb-test-variant-selector', UmbTestVariantSelectorElement);

// Mirrors the legacy pattern real subclasses (ours, previously, and any external one) used to set the
// sorter: a class field override, not an assignment inside a method body. This must still compile — see
// the design note on `_variantSorter` for why it stays a plain field rather than an accessor.
class UmbLegacyOverrideVariantSelectorElement extends UmbTestVariantSelectorElement {
	protected override _variantSorter = sorterA;
}
customElements.define('umb-test-legacy-override-variant-selector', UmbLegacyOverrideVariantSelectorElement);

describe('UmbWorkspaceSplitViewVariantSelectorElement variant sorter', () => {
	let element: UmbTestVariantSelectorElement;

	beforeEach(async () => {
		element = await fixture(html`<umb-test-variant-selector></umb-test-variant-selector>`);
	});

	it('defaults variantSorter to a no-op sorter', () => {
		expect(element.variantSorter({} as VariantOption, {} as VariantOption)).to.equal(0);
	});

	it('defaults the deprecated _variantSorter to the same no-op sorter', () => {
		expect(element.deprecatedVariantSorter({} as VariantOption, {} as VariantOption)).to.equal(0);
	});

	it('setting variantSorter directly is reflected on the property', () => {
		element.variantSorter = sorterA;
		expect(element.variantSorter).to.equal(sorterA);
	});

	it('setting the deprecated _variantSorter directly is reflected on that same field, independently of variantSorter', () => {
		element.deprecatedVariantSorter = sorterA;
		expect(element.deprecatedVariantSorter).to.equal(sorterA);
		expect(element.variantSorter).to.not.equal(sorterA);
	});

	it('a legacy field-override of the deprecated _variantSorter (the old subclass pattern) still compiles and sets the field', async () => {
		const legacyElement: UmbLegacyOverrideVariantSelectorElement = await fixture(
			html`<umb-test-legacy-override-variant-selector></umb-test-legacy-override-variant-selector>`,
		);

		expect(legacyElement.deprecatedVariantSorter).to.equal(sorterA);
	});
});
