import { UmbPropertyEditorUIContentPickerElement } from './property-editor-ui-content-picker.element.js';
import type { UmbContentPickerSource } from './types.js';
import { aTimeout, expect, fixture, html } from '@open-wc/testing';
import { UmbContextProvider } from '@umbraco-cms/backoffice/context-api';
import { UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';
import { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import { UMB_MEDIA_ENTITY_TYPE } from '@umbraco-cms/backoffice/media';
import {
	UMB_CLIPBOARD_PROPERTY_CONTEXT,
	UmbClipboardCopyRequestEvent,
	UmbClipboardPasteRequestEvent,
} from '@umbraco-cms/backoffice/clipboard';
import { type UmbTestRunnerWindow, defaultA11yConfig } from '@umbraco-cms/internal/test-utils';

function mediaReference(unique: string) {
	return { type: UMB_MEDIA_ENTITY_TYPE, unique };
}

describe('UmbPropertyEditorUIContentPickerElement', () => {
	let element: UmbPropertyEditorUIContentPickerElement;

	beforeEach(async () => {
		element = await fixture(html` <umb-property-editor-ui-content-picker></umb-property-editor-ui-content-picker> `);
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbPropertyEditorUIContentPickerElement);
	});

	if ((window as UmbTestRunnerWindow).__UMBRACO_TEST_RUN_A11Y_TEST) {
		it('passes the a11y audit', async () => {
			await expect(element).shadowDom.to.be.accessible(defaultA11yConfig);
		});
	}

	describe('clipboard', () => {
		let pasteResult: Array<Array<{ type: string; unique: string }>>;
		let written: Array<{ propertyValue: unknown; itemName?: string; icon?: string }>;

		beforeEach(async () => {
			pasteResult = [];
			written = [];

			// Only the members the element uses; the real context is covered by its own tests.
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

		function setConfig(startNodeType: UmbContentPickerSource['type'], maxNumber?: number) {
			element.config = new UmbPropertyEditorConfigCollection([
				{ alias: 'startNode', value: { type: startNodeType } },
				{ alias: 'maxNumber', value: maxNumber },
			]);
		}

		// The event comes from the content input, which is where the property editor listens for it.
		async function dispatchFromInput(event: Event) {
			await element.updateComplete;
			element.shadowRoot!.querySelector('umb-input-content')!.dispatchEvent(event);
			await aTimeout(0);
		}

		function clipboardConfigOfInput() {
			return element.shadowRoot!.querySelector('umb-input-content')!.mediaClipboardConfig;
		}

		describe('the configuration handed to the input', () => {
			it('offers copy and paste when the picker is configured for media', async () => {
				setConfig('media');
				await element.updateComplete;

				expect(clipboardConfigOfInput()?.copy.enabled).to.be.true;
				expect(clipboardConfigOfInput()?.paste.enabled).to.be.true;
				expect(clipboardConfigOfInput()?.paste.types).to.deep.equal(['media']);
			});

			it('offers nothing when the picker is configured for content', async () => {
				setConfig('content');
				await element.updateComplete;

				// Every entry value type this editor can translate is a media one, so there is nothing to offer.
				expect(clipboardConfigOfInput()).to.be.undefined;
			});

			it('offers nothing when the picker is configured for members', async () => {
				setConfig('member');
				await element.updateComplete;

				expect(clipboardConfigOfInput()).to.be.undefined;
			});
		});

		describe('copy', () => {
			it('writes the value of the requested item, identified by its unique', async () => {
				setConfig('media');
				element.value = [mediaReference('media-a'), mediaReference('media-b')];

				await dispatchFromInput(
					new UmbClipboardCopyRequestEvent({ unique: 'media-b', name: 'Media B', icon: 'icon-picture' }),
				);

				expect(written).to.have.lengthOf(1);
				expect(written[0].propertyValue).to.deep.equal([mediaReference('media-b')]);
				expect(written[0].itemName).to.equal('Media B');
				expect(written[0].icon).to.equal('icon-picture');
			});
		});

		describe('paste', () => {
			it('appends the pasted references', async () => {
				setConfig('media');
				element.value = [mediaReference('media-a')];
				pasteResult = [[mediaReference('media-b')]];

				await dispatchFromInput(new UmbClipboardPasteRequestEvent(['entry-unique']));

				expect(element.value?.map((x) => x.unique)).to.deep.equal(['media-a', 'media-b']);
			});

			it('does not add a reference that is already picked', async () => {
				setConfig('media');
				element.value = [mediaReference('media-a')];
				pasteResult = [[mediaReference('media-a'), mediaReference('media-b')]];

				await dispatchFromInput(new UmbClipboardPasteRequestEvent(['entry-unique']));

				expect(element.value?.map((x) => x.unique)).to.deep.equal(['media-a', 'media-b']);
			});

			it('adds every reference in the entry, leaving an over-long selection to validation', async () => {
				setConfig('media', 2);
				element.value = [mediaReference('media-a')];
				pasteResult = [[mediaReference('media-b'), mediaReference('media-c')]];

				await dispatchFromInput(new UmbClipboardPasteRequestEvent(['entry-unique']));

				// As when picking: the selection may exceed the configured maximum, and validation reports it.
				expect(element.value?.map((x) => x.unique)).to.deep.equal(['media-a', 'media-b', 'media-c']);
			});
		});
	});
});
