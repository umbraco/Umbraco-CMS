import { UmbCollectionFilterFieldElement } from './collection-filter-field.element.js';
import { UmbDefaultCollectionContext } from '../default/collection-default.context.js';
import { aTimeout, expect, fixture, html } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';
import type { UUIInputElement } from '@umbraco-cms/backoffice/external/uui';

// Host element that provides a collection context for the filter field to consume.
@customElement('test-collection-filter-field-host')
class UmbTestCollectionFilterFieldHostElement extends UmbElementMixin(HTMLElement) {
	collectionContext = new UmbDefaultCollectionContext(this, '');
}

describe('UmbCollectionFilterFieldElement', () => {
	let element: UmbCollectionFilterFieldElement;
	let collectionContext: UmbDefaultCollectionContext;

	const getInput = () => element.shadowRoot!.querySelector<UUIInputElement>('uui-input')!;

	const typeIntoField = (value: string) => {
		const input = getInput();
		input.value = value;
		input.dispatchEvent(new Event('input'));
	};

	const settle = async () => {
		await aTimeout(0);
		await element.updateComplete;
	};

	beforeEach(async () => {
		const host = await fixture<UmbTestCollectionFilterFieldHostElement>(
			html`<test-collection-filter-field-host>
				<umb-collection-filter-field></umb-collection-filter-field>
			</test-collection-filter-field-host>`,
		);
		element = host.querySelector('umb-collection-filter-field')!;
		collectionContext = host.collectionContext;
		await settle();
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbCollectionFilterFieldElement);
	});

	it('populates the field with the filter of the collection', async () => {
		collectionContext.setFilter({ filter: 'news' });

		await settle();

		expect(getInput().value).to.equal('news');
	});

	it('does not overwrite what the user is typing before it has been committed', async () => {
		typeIntoField('abc');

		collectionContext.setFilter({ filter: 'news' });
		await settle();

		expect(getInput().value).to.equal('abc');
	});

	it('populates the field when the filter changes after the user input has been committed', async () => {
		typeIntoField('abc');
		// The field writes to the collection debounced; only then may it accept values from the collection again.
		await aTimeout(600);

		collectionContext.setFilter({ filter: 'news' });
		await settle();

		expect(getInput().value).to.equal('news');
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}
});
