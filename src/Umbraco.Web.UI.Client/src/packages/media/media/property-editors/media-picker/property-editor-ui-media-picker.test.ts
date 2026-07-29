import { UmbPropertyEditorUIMediaPickerElement } from './property-editor-ui-media-picker.element.js';
import type { UmbMediaPickerValueModel } from '../types.js';
import { aTimeout, expect, fixture, html } from '@open-wc/testing';
import { UmbContextProvider } from '@umbraco-cms/backoffice/context-api';
import { UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';
import { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import {
	UMB_CLIPBOARD_PROPERTY_CONTEXT,
	UmbClipboardEntriesPickedEvent,
	UmbClipboardCopyRequestEvent,
} from '@umbraco-cms/backoffice/clipboard';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

function entry(mediaKey: string, key = `key-of-${mediaKey}`) {
	return { key, mediaKey, mediaTypeAlias: 'image', crops: [], focalPoint: null };
}

describe('UmbPropertyEditorUIMediaPickerElement', () => {
	let element: UmbPropertyEditorUIMediaPickerElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-property-editor-ui-media-picker></umb-property-editor-ui-media-picker> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPropertyEditorUIMediaPickerElement);
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}

	describe('clipboard', () => {
		let written: Array<{ propertyValue: unknown; itemName?: string; icon?: string }>;
		let pasteResult: Array<UmbMediaPickerValueModel>;

		beforeEach(async () => {
			written = [];
			pasteResult = [];

			// Only the members the element uses. Translating entries and resolving availability belong to the real
			// clipboard property context, which is covered by its own tests.
			const clipboardContext = {
				getHostElement: () => element,
				copyAvailable: new UmbBooleanState(true).asObservable(),
				pasteAvailable: new UmbBooleanState(true).asObservable(),
				getSupportedPasteEntryValueTypes: async () => ['media'],
				isEntryPastable: async () => true,
				readMultiple: async () => pasteResult,
				write: async (args: { propertyValue: unknown; itemName?: string; icon?: string }) => {
					written.push(args);
					return undefined;
				},
			} as any;

			new UmbContextProvider(element, UMB_CLIPBOARD_PROPERTY_CONTEXT, clipboardContext).hostConnected();
			await aTimeout(0);
		});

		function setMultiple(multiple: boolean) {
			element.config = new UmbPropertyEditorConfigCollection([{ alias: 'multiple', value: multiple }]);
		}

		// Both events come from the input in production, and the property editor listens for them on it.
		async function dispatchFromInput(event: Event) {
			await element.updateComplete;
			element.shadowRoot!.querySelector('umb-input-rich-media')!.dispatchEvent(event);
			await aTimeout(0);
		}

		describe('copy', () => {
			it('writes the value of the requested item, identified by its key', async () => {
				element.value = [entry('media-a'), entry('media-b')];

				await dispatchFromInput(
					new UmbClipboardCopyRequestEvent({ unique: 'key-of-media-b', name: 'Media B', icon: 'icon-picture' }),
				);

				expect(written).to.have.lengthOf(1);
				expect(written[0].propertyValue).to.deep.equal([entry('media-b')]);
				expect(written[0].itemName).to.equal('Media B');
				expect(written[0].icon).to.equal('icon-picture');
			});
		});

		describe('paste', () => {
			it('appends to a multiple picker', async () => {
				setMultiple(true);
				element.value = [entry('media-a')];
				pasteResult = [[entry('media-b')]];

				await dispatchFromInput(new UmbClipboardEntriesPickedEvent(['entry-unique']));

				expect(element.value?.map((x) => x.mediaKey)).to.deep.equal(['media-a', 'media-b']);
			});

			it('does not add media that is already picked', async () => {
				setMultiple(true);
				element.value = [entry('media-a')];
				pasteResult = [[entry('media-a'), entry('media-b')]];

				await dispatchFromInput(new UmbClipboardEntriesPickedEvent(['entry-unique']));

				expect(element.value?.map((x) => x.mediaKey)).to.deep.equal(['media-a', 'media-b']);
			});

			it('adds every media in the entry, leaving an over-long selection to validation', async () => {
				setMultiple(false);
				element.value = [entry('media-a')];
				pasteResult = [[entry('media-b'), entry('media-c')]];

				await dispatchFromInput(new UmbClipboardEntriesPickedEvent(['entry-unique']));

				// As when picking: a single-value picker accepts more than it allows, and validation reports it.
				expect(element.value?.map((x) => x.mediaKey)).to.deep.equal(['media-a', 'media-b', 'media-c']);
			});

			it('keeps the crops and focal point the paste translator resolved', async () => {
				setMultiple(true);
				element.value = [];
				const pasted = {
					...entry('media-b'),
					crops: [{ alias: 'square', width: 100, height: 100 }],
					focalPoint: { left: 0.25, top: 0.75 },
				};
				pasteResult = [[pasted]];

				await dispatchFromInput(new UmbClipboardEntriesPickedEvent(['entry-unique']));

				expect(element.value?.[0].crops).to.deep.equal(pasted.crops);
				expect(element.value?.[0].focalPoint).to.deep.equal(pasted.focalPoint);
			});
		});
	});
});
