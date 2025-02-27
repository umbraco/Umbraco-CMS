import { appendFileExtensionIfNeeded } from './append-file-extension.function.js';
import { expect } from '@open-wc/testing';

describe('append-file-extension-if-needed', () => {
	it('should append extension if not present', () => {
		expect(appendFileExtensionIfNeeded('test', '.js')).to.equal('test.js');
	});

	it('should not append extension if present', () => {
		expect(appendFileExtensionIfNeeded('test.js', '.js')).to.equal('test.js');
	});
});
