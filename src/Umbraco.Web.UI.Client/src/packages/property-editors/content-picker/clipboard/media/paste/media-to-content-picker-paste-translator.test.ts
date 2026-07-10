import { UmbMediaToContentPickerClipboardPastePropertyValueTranslator } from './media-to-content-picker-paste-translator.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UMB_MEDIA_ENTITY_TYPE } from '@umbraco-cms/backoffice/media';
import { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';

@customElement('test-media-to-content-picker-paste-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbMediaToContentPickerClipboardPastePropertyValueTranslator', () => {
	let hostElement: UmbTestControllerHostElement;
	let pasteTranslator: UmbMediaToContentPickerClipboardPastePropertyValueTranslator;

	beforeEach(() => {
		hostElement = new UmbTestControllerHostElement();
		pasteTranslator = new UmbMediaToContentPickerClipboardPastePropertyValueTranslator(hostElement);
		document.body.innerHTML = '';
		document.body.appendChild(hostElement);
	});

	it('maps each media reference to a media content picker reference', async () => {
		const result = await pasteTranslator.translate([{ unique: 'media-1' }, { unique: 'media-2' }]);
		expect(result).to.deep.equal([
			{ type: UMB_MEDIA_ENTITY_TYPE, unique: 'media-1' },
			{ type: UMB_MEDIA_ENTITY_TYPE, unique: 'media-2' },
		]);
	});

	it('throws when the value is missing', async () => {
		let error: unknown;
		try {
			await pasteTranslator.translate(undefined as never);
		} catch (e) {
			error = e;
		}
		expect(error).to.be.instanceOf(Error);
	});

	it('is compatible when the content picker is configured for media', async () => {
		const config = new UmbPropertyEditorConfigCollection([{ alias: 'startNode', value: { type: 'media' } }]);
		expect(await pasteTranslator.isCompatibleValue([], config)).to.be.true;
	});

	it('is not compatible when the content picker is configured for content', async () => {
		const config = new UmbPropertyEditorConfigCollection([{ alias: 'startNode', value: { type: 'content' } }]);
		expect(await pasteTranslator.isCompatibleValue([], config)).to.be.false;
	});

	it('is not compatible when no configuration is provided', async () => {
		expect(await pasteTranslator.isCompatibleValue([], undefined)).to.be.false;
	});
});
