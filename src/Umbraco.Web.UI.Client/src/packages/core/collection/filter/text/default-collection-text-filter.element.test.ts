import { UmbDefaultCollectionTextFilterElement } from './default-collection-text-filter.element.js';
import { UmbDefaultCollectionTextFilterApi } from './default-collection-text-filter.api.js';
import { UmbDefaultCollectionContext } from '../../default/collection-default.context.js';
import { aTimeout, expect, fixture, html } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import type { UUIInputElement } from '@umbraco-cms/backoffice/external/uui';

@customElement('test-default-collection-text-filter-host')
class UmbTestDefaultCollectionTextFilterHostElement extends UmbElementMixin(HTMLElement) {
	collectionContext = new UmbDefaultCollectionContext(this, '');
}

describe('UmbDefaultCollectionTextFilterElement', () => {
	let element: UmbDefaultCollectionTextFilterElement;
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
		const host = await fixture<UmbTestDefaultCollectionTextFilterHostElement>(
			html`<test-default-collection-text-filter-host>
				<umb-default-collection-text-filter></umb-default-collection-text-filter>
			</test-default-collection-text-filter-host>`,
		);
		element = host.querySelector('umb-default-collection-text-filter')!;
		element.api = new UmbDefaultCollectionTextFilterApi(element);
		collectionContext = host.collectionContext;
		await settle();
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
		// The text is written to the collection debounced; only then may the field accept values from the collection again.
		await aTimeout(600);

		collectionContext.setFilter({ filter: 'news' });
		await settle();

		expect(getInput().value).to.equal('news');
	});
});
