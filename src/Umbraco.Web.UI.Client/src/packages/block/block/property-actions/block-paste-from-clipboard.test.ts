import { UmbBlockPasteFromClipboardPropertyAction } from './block-paste-from-clipboard.js';
import type { UmbBlockValueDataPropertiesBaseType } from '../types.js';
import { expect } from '@open-wc/testing';
import { customElement } from 'lit/decorators.js';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';

@customElement('test-block-paste-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

/** Exposes the protected hook and lets a test stand in a variant without a real property context. */
class TestableAction extends UmbBlockPasteFromClipboardPropertyAction {
	setVariantId(variantId?: UmbVariantId) {
		this._propertyContext = variantId
			? ({ getVariantId: () => variantId } as unknown as typeof this._propertyContext)
			: undefined;
	}

	prepare(value: UmbBlockValueDataPropertiesBaseType) {
		return this._prepareValue(value);
	}
}

describe('UmbBlockPasteFromClipboardPropertyAction', () => {
	let hostElement: UmbTestControllerHostElement;
	let action: TestableAction;

	const value = (): UmbBlockValueDataPropertiesBaseType => ({
		contentData: [
			{ key: 'contentKey1', contentTypeKey: 'contentTypeKey', values: [] },
			{ key: 'contentKey2', contentTypeKey: 'contentTypeKey', values: [] },
		],
		settingsData: [],
		expose: [],
	});

	beforeEach(async () => {
		hostElement = new UmbTestControllerHostElement();
		document.body.innerHTML = '';
		document.body.appendChild(hostElement);
		action = new TestableAction(hostElement, { meta: {} } as never);
	});

	it('exposes every pasted block for the variant being edited', async () => {
		action.setVariantId(UmbVariantId.Create({ culture: 'en-US', segment: null }));

		const result = await action.prepare(value());

		expect(result.expose).to.deep.equal([
			{ contentKey: 'contentKey1', culture: 'en-US', segment: null },
			{ contentKey: 'contentKey2', culture: 'en-US', segment: null },
		]);
	});

	it('exposes as invariant when the property is invariant', async () => {
		action.setVariantId(UmbVariantId.CreateInvariant());

		const result = await action.prepare(value());

		expect(result.expose).to.deep.equal([
			{ contentKey: 'contentKey1', culture: null, segment: null },
			{ contentKey: 'contentKey2', culture: null, segment: null },
		]);
	});

	it('does not touch a value without content data', async () => {
		action.setVariantId(UmbVariantId.CreateInvariant());

		const empty: UmbBlockValueDataPropertiesBaseType = { contentData: [], settingsData: [], expose: [] };
		const result = await action.prepare(empty);

		expect(result.expose).to.deep.equal([]);
	});
});
