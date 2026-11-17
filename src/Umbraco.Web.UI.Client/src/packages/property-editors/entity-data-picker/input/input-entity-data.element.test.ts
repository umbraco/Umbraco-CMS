import { UmbInputEntityDataElement } from './input-entity-data.element.js';
import { expect, fixture, html } from '@open-wc/testing';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';
import type { UmbLocalizationSet } from '@umbraco-cms/backoffice/localization-api';
import { UMB_DEFAULT_LOCALIZATION_CULTURE, umbLocalizationManager } from '@umbraco-cms/backoffice/localization-api';

describe('UmbInputEntityDataElement', () => {
	let element: UmbInputEntityDataElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-input-entity-data></umb-input-entity-data> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbInputEntityDataElement);
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

	// The server reports a violated item count with these messages, so the input has to use the same ones
	// for the editor to see the same wording whichever side validated the value.
	describe('item count validation messages', () => {
		const itemCountMessages: UmbLocalizationSet = {
			$code: UMB_DEFAULT_LOCALIZATION_CULTURE,
			$dir: 'ltr',
			validation_entriesShort: 'Minimum %0% entries, requires %1% more.',
			validation_entriesExceed: 'Maximum %0% entries, you have entered %1% too many.',
		};

		let previousFallback: ReturnType<typeof umbLocalizationManager.localizations.get>;

		beforeEach(() => {
			previousFallback = umbLocalizationManager.localizations.get(UMB_DEFAULT_LOCALIZATION_CULTURE);
			umbLocalizationManager.registerLocalization(itemCountMessages);
		});

		afterEach(() => {
			if (previousFallback) {
				umbLocalizationManager.localizations.set(UMB_DEFAULT_LOCALIZATION_CULTURE, previousFallback);
			} else {
				umbLocalizationManager.localizations.delete(UMB_DEFAULT_LOCALIZATION_CULTURE);
			}
		});

		it('states the minimum and how many more items are needed', async () => {
			element.min = 3;
			element.selection = ['a'];
			await element.updateComplete;

			expect(element.checkValidity()).to.equal(false);
			expect(element.validationMessage).to.equal('Minimum 3 entries, requires 2 more.');
		});

		it('states the maximum and how many items are over it', async () => {
			element.max = 1;
			element.selection = ['a', 'b', 'c'];
			await element.updateComplete;

			expect(element.checkValidity()).to.equal(false);
			expect(element.validationMessage).to.equal('Maximum 1 entries, you have entered 2 too many.');
		});
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}
});
