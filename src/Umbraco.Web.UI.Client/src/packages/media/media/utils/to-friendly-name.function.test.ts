import { toFriendlyName } from './to-friendly-name.function.js';
import { expect } from '@open-wc/testing';

describe('toFriendlyName', () => {
	it('strips the extension and title-cases dashed words', () => {
		expect(toFriendlyName('my-image-file.jpg')).to.eq('My Image File');
	});

	it('strips the extension and title-cases underscored words', () => {
		expect(toFriendlyName('snake_case_file.PNG')).to.eq('Snake Case File');
	});

	it('handles a filename that already contains spaces', () => {
		expect(toFriendlyName('already nice.pdf')).to.eq('Already Nice');
	});

	it('handles a mix of separators', () => {
		expect(toFriendlyName('mixed_separator-name.docx')).to.eq('Mixed Separator Name');
	});

	it('treats each all-uppercase word as an acronym, including single-letter words', () => {
		expect(toFriendlyName('JUST-A-FILE.jpg')).to.eq('JUST A FILE');
	});

	it('preserves all-uppercase words as acronyms', () => {
		expect(toFriendlyName('NASA-photo.jpg')).to.eq('NASA Photo');
	});

	it('collapses consecutive separators', () => {
		expect(toFriendlyName('multiple--dashes__here.jpg')).to.eq('Multiple Dashes Here');
	});

	it('returns the friendly name when there is no extension', () => {
		expect(toFriendlyName('no-extension')).to.eq('No Extension');
	});

	it('treats dotfiles as having no extension', () => {
		expect(toFriendlyName('.gitignore')).to.eq('.Gitignore');
	});

	it('only strips the last extension for multi-dot names', () => {
		expect(toFriendlyName('my.file.name.jpg')).to.eq('My.File.Name');
	});

	it('returns an empty string for empty input', () => {
		expect(toFriendlyName('')).to.eq('');
	});

	it('returns an empty string for whitespace-only input', () => {
		expect(toFriendlyName('   ')).to.eq('');
	});

	it('collapses interior whitespace', () => {
		expect(toFriendlyName('hello   world.jpg')).to.eq('Hello World');
	});

	it('strips a trailing dot (parity with StringExtensions.StripFileExtension)', () => {
		expect(toFriendlyName('file.')).to.eq('File');
	});

	it('does not strip when the "extension" contains whitespace', () => {
		// StringExtensions.StripFileExtension preserves these names rather than treating
		// the last dot as an extension delimiter, so the dot survives into the output.
		expect(toFriendlyName('report.final draft')).to.eq('Report.Final Draft');
	});

	it('preserves the extension when trailing whitespace is part of the extension span', () => {
		// Same StripFileExtension rule: trailing whitespace after the dot lands inside
		// the extension span, so the dot is not treated as a delimiter and the
		// title-cased extension survives into the output.
		expect(toFriendlyName('  spaced-name.jpg  ')).to.eq('Spaced Name.Jpg');
	});

	it('handles Latin-extended letters as part of words', () => {
		expect(toFriendlyName('école-maternelle.jpg')).to.eq('École Maternelle');
	});

	it('handles accented letters mid-word', () => {
		expect(toFriendlyName('café-photo.jpg')).to.eq('Café Photo');
	});

	it('handles Cyrillic letters', () => {
		expect(toFriendlyName('файл-имя.jpg')).to.eq('Файл Имя');
	});
});
