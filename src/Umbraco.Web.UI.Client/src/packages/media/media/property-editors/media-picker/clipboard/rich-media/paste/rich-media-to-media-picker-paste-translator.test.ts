import { UmbRichMediaToMediaPickerClipboardPastePropertyValueTranslator } from './rich-media-to-media-picker-paste-translator.js';
import type { UmbRichMediaClipboardEntryValueModel } from '../../../../../clipboard/types.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';

@customElement('test-rich-media-to-media-picker-paste-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbRichMediaToMediaPickerClipboardPastePropertyValueTranslator', () => {
	let hostElement: UmbTestControllerHostElement;
	let pasteTranslator: UmbRichMediaToMediaPickerClipboardPastePropertyValueTranslator;

	const clipboardValue: UmbRichMediaClipboardEntryValueModel = [
		{ unique: 'media-1', focalPoint: { left: 0.4, top: 0.6 }, crops: [{ alias: 'square', width: 100, height: 100 }] },
	];

	beforeEach(() => {
		hostElement = new UmbTestControllerHostElement();
		pasteTranslator = new UmbRichMediaToMediaPickerClipboardPastePropertyValueTranslator(hostElement);
		document.body.innerHTML = '';
		document.body.appendChild(hostElement);
	});

	it('maps each rich media reference to a media picker entry, preserving crops and focal point', async () => {
		const result = await pasteTranslator.translate(clipboardValue);
		expect(result).to.have.lengthOf(1);
		const [entry] = result;
		expect(entry.key).to.be.a('string');
		expect(entry.key.length).to.be.greaterThan(0);
		expect(entry.mediaKey).to.equal('media-1');
		expect(entry.mediaTypeAlias).to.equal('');
		expect(entry.focalPoint).to.deep.equal({ left: 0.4, top: 0.6 });
		expect(entry.crops).to.deep.equal([{ alias: 'square', width: 100, height: 100 }]);
	});

	it('generates a new unique key per entry', async () => {
		const result = await pasteTranslator.translate([
			{ unique: 'a', focalPoint: null, crops: [] },
			{ unique: 'b', focalPoint: null, crops: [] },
		]);
		expect(result[0].key).to.not.equal(result[1].key);
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

	it('accepts any rich media value (isCompatibleValue)', async () => {
		expect(await pasteTranslator.isCompatibleValue()).to.be.true;
	});
});
