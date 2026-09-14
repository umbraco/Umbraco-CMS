import { UmbMediaToMediaPickerClipboardPastePropertyValueTranslator } from './media-to-media-picker-paste-translator.js';
import type { UmbMediaClipboardEntryValueModel } from '../../../../../clipboard/types.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';

@customElement('test-media-to-media-picker-paste-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbMediaToMediaPickerClipboardPastePropertyValueTranslator', () => {
	let hostElement: UmbTestControllerHostElement;
	let pasteTranslator: UmbMediaToMediaPickerClipboardPastePropertyValueTranslator;

	const clipboardValue: UmbMediaClipboardEntryValueModel = [{ unique: 'media-1' }];

	beforeEach(() => {
		hostElement = new UmbTestControllerHostElement();
		pasteTranslator = new UmbMediaToMediaPickerClipboardPastePropertyValueTranslator(hostElement);
		document.body.innerHTML = '';
		document.body.appendChild(hostElement);
	});

	it('maps each media reference to a media picker entry with empty crops and no focal point', async () => {
		const result = await pasteTranslator.translate(clipboardValue);
		expect(result).to.have.lengthOf(1);
		const [entry] = result;
		expect(entry.key).to.be.a('string');
		expect(entry.key.length).to.be.greaterThan(0);
		expect(entry.mediaKey).to.equal('media-1');
		expect(entry.mediaTypeAlias).to.equal('');
		expect(entry.focalPoint).to.equal(null);
		expect(entry.crops).to.deep.equal([]);
	});

	it('generates a new unique key per entry', async () => {
		const result = await pasteTranslator.translate([{ unique: 'a' }, { unique: 'b' }]);
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

	it('accepts any media value (isCompatibleValue)', async () => {
		expect(await pasteTranslator.isCompatibleValue()).to.be.true;
	});
});
