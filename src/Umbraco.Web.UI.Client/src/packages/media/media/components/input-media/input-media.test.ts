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

	describe('interactionMemories', () => {
		it('seeds the picker context from the incoming snapshot', () => {
			element.interactionMemories = [{ unique: 'a' }, { unique: 'b' }];
			expect(element.interactionMemories?.map((memory) => memory.unique)).to.eql(['a', 'b']);
		});

		it('removes memories that are no longer present when the snapshot shrinks', () => {
			element.interactionMemories = [{ unique: 'a' }, { unique: 'b' }];
			element.interactionMemories = [{ unique: 'a' }];
			expect(element.interactionMemories?.map((memory) => memory.unique)).to.eql(['a']);
		});

		it('clears all memories when the snapshot is emptied', () => {
			element.interactionMemories = [{ unique: 'a' }, { unique: 'b' }];
			element.interactionMemories = [];
			expect(element.interactionMemories).to.eql([]);
		});
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}
});
