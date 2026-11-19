import { UmbInputRichMediaElement } from './input-rich-media.element.js';
import { expect, fixture, html } from '@open-wc/testing';
import { UmbClipboardCopyRequestEvent } from '@umbraco-cms/backoffice/clipboard';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

describe('UmbInputRichMediaElement', () => {
	let element: UmbInputRichMediaElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-input-rich-media></umb-input-rich-media> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbInputRichMediaElement);
	});

	describe('clipboard copy', () => {
		async function renderCard() {
			element.value = [
				{ key: 'entry-key', mediaKey: 'media-unique', mediaTypeAlias: 'image', crops: [], focalPoint: null },
			];
			await element.updateComplete;
		}

		function copyAction() {
			return element.shadowRoot?.querySelector('uui-icon[name="icon-clipboard-copy"]');
		}

		it('offers no copy action when the host supplies no clipboard configuration', async () => {
			await renderCard();
			expect(copyAction()).to.be.null;
		});

		it('offers no copy action when the host cannot copy', async () => {
			element.clipboardConfig = { copy: { enabled: false }, paste: { enabled: false, types: [] } };
			await renderCard();
			expect(copyAction()).to.be.null;
		});

		it('offers no copy action when readonly', async () => {
			element.clipboardConfig = { copy: { enabled: true }, paste: { enabled: false, types: [] } };
			element.readonly = true;
			await renderCard();
			expect(copyAction()).to.be.null;
		});

		it('offers a copy action when the host can copy', async () => {
			element.clipboardConfig = { copy: { enabled: true }, paste: { enabled: false, types: [] } };
			await renderCard();
			expect(copyAction()).to.not.be.null;
		});

		it('reports the identity of the item, leaving the value to the host', async () => {
			element.clipboardConfig = { copy: { enabled: true }, paste: { enabled: false, types: [] } };
			await renderCard();

			const events: Array<UmbClipboardCopyRequestEvent> = [];
			element.addEventListener(UmbClipboardCopyRequestEvent.TYPE, (event) =>
				events.push(event as UmbClipboardCopyRequestEvent),
			);

			copyAction()!.parentElement!.dispatchEvent(new MouseEvent('click', { bubbles: true, composed: true }));

			// The key of the value entry, not the media unique: that is how the host finds the item in its value.
			expect(events).to.have.lengthOf(1);
			expect(events[0].unique).to.equal('entry-key');
		});
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
