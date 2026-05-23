import { UmbInputMediaElement } from './input-media.element.js';
import { expect, fixture, html } from '@open-wc/testing';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbInputMediaElement', () => {
	let element: UmbInputMediaElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-input-media></umb-input-media> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbInputMediaElement);
	});

	it('renders selected cards as disabled when no parent route context is reachable', async () => {
		// Without a parent route context (e.g. when this input is rendered inside a non-routable modal),
		// the workspace edit path stays empty and the picked card should render `disabled` to suppress the
		// misleading hover/cursor/underline styling. The default fixture has no umb-router-slot ancestor,
		// so this is the route-context-less state. `_cards` is set directly to bypass the picker
		// context's repository fetch.
		// eslint-disable-next-line @typescript-eslint/no-explicit-any
		(element as any)._cards = [
			{
				unique: 'test-unique',
				name: 'Test Media',
				entityType: 'media',
				mediaType: { unique: 'media-type', icon: 'icon-picture', collection: null },
				hasChildren: false,
				isTrashed: false,
				parent: null,
				variants: [],
			},
		];
		await element.updateComplete;

		const card = element.shadowRoot?.querySelector('uui-card-media');
		expect(card, 'expected a uui-card-media to be rendered').to.exist;
		expect(card?.hasAttribute('disabled'), 'expected card to be disabled when no route context is reachable').to.be
			.true;
		expect(card?.hasAttribute('href'), 'expected card href to be omitted when no route context is reachable').to.be
			.false;
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}
});
